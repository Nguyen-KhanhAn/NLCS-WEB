//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Demo_Hangfire.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Addresses = new HashSet<Address>();
            this.Bills = new HashSet<Bill>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Nullable<int> Province_id { get; set; }
        public Nullable<int> District_id { get; set; }
        public Nullable<int> Town_id { get; set; }
        public Nullable<int> TypeUser_id { get; set; }
        public string Phone { get; set; }
        public Nullable<int> Captcha { get; set; }
        public Nullable<bool> IsChecked { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Address> Addresses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual District District { get; set; }
        public virtual Province Province { get; set; }
        public virtual Town Town { get; set; }
        public virtual TypeUser TypeUser { get; set; }
    }
}
