# Rebuild the vector wordmark and browser icons using the local Segoe UI font.
# Run from the repository root on Windows. SVG text is converted to paths.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$destination = Join-Path $PSScriptRoot '../Prodora.WebUI/wwwroot/brand'
New-Item -ItemType Directory -Force -Path $destination | Out-Null
$invariant = [Globalization.CultureInfo]::InvariantCulture
function Number([single]$value) { return $value.ToString('0.###', $invariant) }
function SvgPath($path) {
    $points = $path.PathPoints
    $types = $path.PathTypes
    $builder = [Text.StringBuilder]::new()
    for ($i = 0; $i -lt $points.Length; $i++) {
        $kind = $types[$i] -band 7
        if ($kind -eq 0) { [void]$builder.Append("M$(Number $points[$i].X) $(Number $points[$i].Y)") }
        elseif ($kind -eq 1) { [void]$builder.Append("L$(Number $points[$i].X) $(Number $points[$i].Y)") }
        elseif ($kind -eq 3) {
            [void]$builder.Append("C$(Number $points[$i].X) $(Number $points[$i].Y) $(Number $points[$i+1].X) $(Number $points[$i+1].Y) $(Number $points[$i+2].X) $(Number $points[$i+2].Y)")
            $i += 2
        }
        if (($types[$i] -band 128) -ne 0) { [void]$builder.Append('Z') }
    }
    return $builder.ToString()
}
$font = [Drawing.FontFamily]::new('Segoe UI')
$black = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#191919'))
$gold = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#efb33f'))
$word = [Drawing.Drawing2D.GraphicsPath]::new()
$word.AddString('Prodora', $font, [int][Drawing.FontStyle]::Bold, 72, [Drawing.PointF]::new(0,0), [Drawing.StringFormat]::GenericTypographic)
$bounds = $word.GetBounds()
$move = [Drawing.Drawing2D.Matrix]::new()
$move.Translate(21-$bounds.X, 6-$bounds.Y)
$word.Transform($move)
$width = [Math]::Ceiling($bounds.Width + 29)
$height = [Math]::Ceiling($bounds.Height + 12)
$dotY = $height - 11
$wordSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ' + $width + ' ' + $height + '" role="img" aria-labelledby="title"><title id="title">.Prodora</title><circle cx="7" cy="' + $dotY + '" r="5.5" fill="#efb33f"/><path d="' + (SvgPath $word) + '" fill="#191919"/></svg>'
[IO.File]::WriteAllText((Join-Path $destination 'prodora-wordmark.svg'), $wordSvg, [Text.UTF8Encoding]::new($false))
$letter = [Drawing.Drawing2D.GraphicsPath]::new()
$letter.AddString('P', $font, [int][Drawing.FontStyle]::Bold, 50, [Drawing.PointF]::new(0,0), [Drawing.StringFormat]::GenericTypographic)
$bounds = $letter.GetBounds()
$move.Reset()
$move.Translate(23-$bounds.X, 13-$bounds.Y)
$letter.Transform($move)
$iconSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" role="img" aria-labelledby="title"><title id="title">Prodora</title><rect width="64" height="64" rx="14" fill="#f5f5f2"/><circle cx="13" cy="46" r="4.5" fill="#efb33f"/><path d="' + (SvgPath $letter) + '" fill="#191919"/></svg>'
[IO.File]::WriteAllText((Join-Path $destination 'prodora-icon.svg'), $iconSvg, [Text.UTF8Encoding]::new($false))
foreach ($size in @(32,180)) {
    $bitmap = [Drawing.Bitmap]::new($size,$size)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#f5f5f2'))
    $graphics.ScaleTransform($size/64.0,$size/64.0)
    $graphics.FillPath($black,$letter)
    $graphics.FillEllipse($gold,[single]8.5,[single]41.5,[single]9,[single]9)
    if ($size -eq 180) { $bitmap.Save((Join-Path $destination 'apple-touch-icon.png'), [Drawing.Imaging.ImageFormat]::Png) }
    else {
        $icon = [Drawing.Icon]::FromHandle($bitmap.GetHicon())
        $stream = [IO.File]::Create((Join-Path $destination '../favicon.ico'))
        try { $icon.Save($stream) } finally { $stream.Dispose(); $icon.Dispose() }
    }
    $graphics.Dispose()
    $bitmap.Dispose()
}
$preview = [Drawing.Bitmap]::new([int]($width*2),[int]($height*2))
$graphics = [Drawing.Graphics]::FromImage($preview)
$graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([Drawing.Color]::White)
$graphics.ScaleTransform(2,2)
$graphics.FillPath($black,$word)
$graphics.FillEllipse($gold,[single]1.5,[single]($dotY-5.5),[single]11,[single]11)
$preview.Save((Join-Path $destination 'prodora-wordmark.png'), [Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose(); $preview.Dispose(); $word.Dispose(); $letter.Dispose(); $move.Dispose(); $font.Dispose(); $black.Dispose(); $gold.Dispose()
Write-Output 'Brand SVGs, PNGs and favicon generated.'
