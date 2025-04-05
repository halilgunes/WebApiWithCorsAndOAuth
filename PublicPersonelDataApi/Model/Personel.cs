using System.ComponentModel.DataAnnotations;

namespace PublicPersonelDataApi.Model
{
    public class Personel
    {
        [Key]
        [Range(1, 300, ErrorMessage = "Id alanı 1 ile 300 arasında bir değer olmalıdır.")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Name en fazla 100 karakter olabilir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Title alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Title en fazla 50 karakter olabilir.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Phone alanı zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string Phone { get; set; }

    }
}
