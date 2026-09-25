namespace Prodora.WebUI.Models
{
	public class CommentModel
	{
		public int Id { get; set; }
		[System.ComponentModel.DataAnnotations.Required]
		[System.ComponentModel.DataAnnotations.StringLength(500)]
		public string Text { get; set; }
		public int? ProductId { get; set; }
		[System.ComponentModel.DataAnnotations.Range(1, 5)]
		public int Raiting { get; set; }

	}
}
