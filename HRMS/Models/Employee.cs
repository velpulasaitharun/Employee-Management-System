using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HRMS.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression(@"^[A-Za-z]+$", ErrorMessage = "First Name should contain only alphabets")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "Last Name should contain only alphabets")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select Gender")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(
         @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
         ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }= string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression(
        @"^[6-9]\d{9}$",
        ErrorMessage = "Phone number must start with 6-9 and contain exactly 10 digits")]
        public string PhoneNo { get; set; }= string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
        public string Address { get; set; }= string.Empty;

        [Required(ErrorMessage = "Joining Date is required")]

        public DateTime? JoiningDate { get; set; }


        [Required(ErrorMessage = "Salary is required")]
        [Range(1, 99999999, ErrorMessage = "Enter valid salary")]

        public decimal? Salary { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required(ErrorMessage = "Please Select Department")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Department")]

        [ForeignKey("Department")]
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        [Required(ErrorMessage = "Please Select Designation")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Designation")]

        [ForeignKey("Designation")]
        public int? DesignationId { get; set; }
        public Designation? Designation { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

    }

}
