using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor để khởi tạo DbContext với các tùy chọn
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } // Bộ sưu tập cho truyện
        public DbSet<Comment> Comments { get; set; } // Bộ sưu tập cho bình luận
        public DbSet<ChapterComment> ChapterComments { get; set; } // Bộ sưu tập cho bình luận chương
        public DbSet<Category> Categories { get; set; } // Bộ sưu tập cho thể loại
        public DbSet<Chapter> Chapters { get; set; } // Bộ sưu tập cho chương
        public DbSet<Rating> Ratings { get; set; } // Bộ sưu tập cho đánh giá
        public DbSet<PremiumPackage> PremiumPackages { get; set; } // Bộ sưu tập cho các gói premium
        public DbSet<ProductCategory> ProductCategories { get; set; } // Bảng trung gian
        public DbSet<Follow> Follows { get; set; } // Thêm DbSet cho Follow
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId }); // Khóa chính của bảng trung gian
         
            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductCategories)
                .WithOne(pc => pc.Product)
                .HasForeignKey(pc => pc.ProductId);

            modelBuilder.Entity<Chapter>()
     .HasOne(c => c.Product)
     .WithMany(p => p.Chapters)
     .HasForeignKey(c => c.ProductId)
     .OnDelete(DeleteBehavior.Cascade); // Thực hiện xóa chương khi sản phẩm bị xóa


            modelBuilder.Entity<Product>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa bình luận khi sản phẩm bị xóa


            modelBuilder.Entity<Product>()
              .HasMany(p => p.Chapters)
              .WithOne(c => c.Product)
              .HasForeignKey(c => c.ProductId)
              .OnDelete(DeleteBehavior.Cascade); // Xóa chương khi sản phẩm bị xóa
            modelBuilder.Entity<Chapter>()
                           .HasMany(c => c.ChapterComments)
                           .WithOne(cc => cc.Chapter)
                           .HasForeignKey(cc => cc.ChapterId)
                           .OnDelete(DeleteBehavior.Cascade); // Xóa bình luận chương khi chương bị xóa


            modelBuilder.Entity<PremiumPackage>()
                .Property(p => p.Price)
                .HasPrecision(18, 2); // Độ chính xác 18, quy mô 2

           

            modelBuilder.Entity<Rating>()
        .HasIndex(r => new { r.UserId, r.ProductId })
        .IsUnique();
        }
    }
}
