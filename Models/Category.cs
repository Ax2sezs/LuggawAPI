using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace backend.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string Name_En { get; set; }
        public string Code { get; set; }

        // ✅ Navigation Property - ถ้าอยากเชื่อมกับ Reward
        public ICollection<Rewards> Rewards { get; set; }
    }
}
