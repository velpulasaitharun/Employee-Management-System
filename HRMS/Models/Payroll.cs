using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class Payroll
    {
        [Key]
        public int PayrollId { get; set; }

        public int EmployeeId { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal HRA { get; set; }

        public decimal DA { get; set; }

        public decimal PF { get; set; }

        public decimal ProfessionalTax { get; set; }

        public decimal NetSalary { get; set; }

        public decimal GrossSalary { get; set; }

        public DateTime PayMonth { get; set; } 

        public DateTime GeneratedDate { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
    }
}