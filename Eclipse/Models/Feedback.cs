using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Eclipse.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required, DisplayName("Nota")]
        public int? Nota { get; set; }

        [Required, DisplayName("Comentario")]
        public string? Comentario { get; set; }



    }
}
