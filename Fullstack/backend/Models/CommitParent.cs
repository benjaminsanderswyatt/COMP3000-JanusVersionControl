using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class CommitParent
    {
        [ForeignKey("ChildHash")]
        [StringLength(40)]
        public string ChildHash { get; set; }

        [ForeignKey("ParentHash")]
        [StringLength(40)]
        public string ParentHash { get; set; }


        public Commit Child { get; set; }
        public Commit Parent { get; set; }

    }

}
