namespace Prodora.WebUI.Models
{
	public class CommentModel
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public string UserId { get; set; }
		public int ProductId { get; set; }
		public int Raiting { get; set; }
	}
}
