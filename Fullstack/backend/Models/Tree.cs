using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tree
    {
        [Key]
        [StringLength(40)]
        public string TreeHash { get; set; }

        [Required]
        [MaxLength(255)]
        public string EntryName { get; set; } // File or folder name

        [Required]
        public EntryType EntryType { get; set; } // Blob or Tree

        [Required]
        [StringLength(40)]
        public string EntryHash { get; set; } // Hash of the file or tree



        // Composite Primary Key
        public static void ConfigureEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tree>()
                .HasKey(t => new { t.TreeHash, t.EntryName });

            // Self referencing relationship (Parent-Child)
            modelBuilder.Entity<Tree>()
                .HasOne(t => t.ParentTree)
                .WithMany(t => t.SubTrees)
                .HasForeignKey(t => new { t.ParentTreeHash, t.ParentEntryName })
                .HasPrincipalKey(t => new { t.TreeHash, t.EntryName })
                .OnDelete(DeleteBehavior.Cascade);

            // File relation (if blob)
            modelBuilder.Entity<Tree>()
                .HasOne(t => t.File)
                .WithMany()
                .HasForeignKey(t => t.EntryHash)
                .IsRequired(false);
        }

        // Self referancing parent child relationship
        public string? ParentTreeHash { get; set; }  // Foreign key to parent tree
        public string? ParentEntryName { get; set; }  // Entry name of parent

        [ForeignKey("ParentTreeHash, ParentEntryName")]
        public Tree? ParentTree { get; set; }

        public ICollection<Tree> SubTrees { get; set; } = new List<Tree>();

        // File reference (if blob)
        [ForeignKey("EntryHash")]
        public File? File { get; set; }
    }


    public enum EntryType
    {
        File,
        Tree
    }

}
