using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eclipse.Models
{
    public class Chamado
    {
        [Key]
        public int ChamadoId { get; set; }

        [Required, DisplayName("Título")]
        public string? Titulo { get; set; }

        [Required, DisplayName("Descrição")]
        public string? Descricao { get; set; }

        [Required, DisplayName("Status")]
        public string? Status { get; set; }

        // Agora, campos opcionais (evitam o erro se vierem NULL do banco)
        [DisplayName("Data de Criação")]
        public string? DataCriacaoString { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        [DisplayName("Data de Atualização")]
        public string? DataAtualizacaoString { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}


