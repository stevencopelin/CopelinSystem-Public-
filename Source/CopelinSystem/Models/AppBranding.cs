using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CopelinSystem.Models
{
    [Table("app_branding")]
    public class AppBranding
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("footer_html")]
        public string FooterHtml { get; set; } = string.Empty;

        [Column("is_locked")]
        public bool IsLocked { get; set; }
    }
}
