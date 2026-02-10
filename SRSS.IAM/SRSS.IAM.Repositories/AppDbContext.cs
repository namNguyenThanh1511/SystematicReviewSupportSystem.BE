using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //"Fluent API Configurations" kết hợp với "Reflection".
        protected override void OnModelCreating(ModelBuilder builder)
               => builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
        /*
         Mục đích của việc sử dụng Assembly ở đây là:
            Tự động hóa cấu hình (Automation): Thay vì phải viết thủ công từng dòng builder.Entity<MyEntity>().Has... cho mọi thực thể (entity) trong dự án, bạn có thể tạo các lớp cấu hình riêng biệt (implementing IEntityTypeConfiguration<T>).
            Khám phá (Discovery/Scanning): Phương thức ApplyConfigurationsFromAssembly yêu cầu một đối tượng Assembly để EF Core có thể "quét" (scan) qua toàn bộ tập hợp đó, tìm kiếm tất cả các lớp cấu hình đã được định nghĩa, và áp dụng chúng một cách tự động vào ModelBuilder.
            Tổ chức mã nguồn (Code Organization): Bằng cách này, bạn giữ logic cấu hình cơ sở dữ liệu (ví dụ: tên bảng, ràng buộc khóa ngoại, kiểu dữ liệu) tách biệt khỏi chính các lớp thực thể (entities) hoặc DbContext, giúp mã nguồn sạch sẽ và dễ quản lý hơn.
                Tóm lại:
                    AssemblyReference.Assembly đóng vai trò là điểm khởi đầu để EF Core biết nơi nào để tìm kiếm các quy tắc cấu hình cơ sở dữ liệu mà bạn đã định nghĩa ở những tệp khác trong cùng một dự án (hoặc một dự án phụ thuộc khác).
         */

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<User> Users { get; set; } = default!;
    }
}
