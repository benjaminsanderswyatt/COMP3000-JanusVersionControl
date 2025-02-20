using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class CommitParent
    {
        [ForeignKey("ChildId")]
        public int ChildId { get; set; }

        [ForeignKey("ParentId")]
        public int ParentId { get; set; }


        public Commit Child { get; set; }
        public Commit Parent { get; set; }

    }

}
