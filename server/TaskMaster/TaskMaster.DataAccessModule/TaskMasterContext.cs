using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskMaster.DataAccessModule.Models;


namespace TaskMaster.DataAccessModule;

/// <summary>
/// Контекст базы данных приложения.
/// </summary>
public partial class TaskMasterContext : DbContext
{
	public TaskMasterContext()
	{
	}

	public TaskMasterContext(DbContextOptions<TaskMasterContext> options)
		: base(options)
	{
	}

	/// <summary>
	/// Набор данных для доступных уровней доступа.
	/// </summary>
	public virtual DbSet<DbAccessLevel> AccessLevels { get; set; }

	/// <summary>
	/// Набор данных для досок.
	/// </summary>
	public virtual DbSet<DbBoard> Boards { get; set; }

	/// <summary>
	/// Набор данных для отображений уровней доступа к доскам.
	/// </summary>
	public virtual DbSet<DbBoardAccessLevelMap> BoardAccessLevelMaps { get; set; }

	/// <summary>
	/// Набор данных для отображений истории просмотра досок.
	/// </summary>
	public virtual DbSet<DbBoardViewHistoryMap> BoardViewHistoryMaps { get; set; }

	/// <summary>
	/// Набор данных для карточек.
	/// </summary>
	public virtual DbSet<DbCard> Cards { get; set; }

	/// <summary>
	/// Набор данных для вложений к карточкам.
	/// </summary>
	public virtual DbSet<DbCardAttachment> CardAttachments { get; set; }

	/// <summary>
	/// Набор данных для комментариев к карточкам.
	/// </summary>
	public virtual DbSet<DbCardComment> CardComments { get; set; }

	/// <summary>
	/// Набор данных для списков карточек.
	/// </summary>
	public virtual DbSet<DbCardList> CardLists { get; set; }

	/// <summary>
	/// Набор данных для дизайнов.
	/// </summary>
	public virtual DbSet<DbDesign> Designs { get; set; }

	/// <summary>
	/// Набор данных для файлов.
	/// </summary>
	public virtual DbSet<DbFile> Files { get; set; }

	/// <summary>
	/// Набор данных для ролей.
	/// </summary>
	public virtual DbSet<DbRole> Roles { get; set; }

	/// <summary>
	/// Набор данных для пользователей.
	/// </summary>
	public virtual DbSet<DbUser> Users { get; set; }

	/// <summary>
	/// Конфигурация подключения к базе данных.
	/// </summary>
	/// <param name="optionsBuilder">Построитель опций контекста.</param>
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

	/// <summary>
	/// Конфигурация модели базы данных.
	/// </summary>
	/// <param name="modelBuilder">Построитель модели.</param>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbAccessLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__accessLe__3213E83F6AC045CC");

            entity.ToTable("accessLevel");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type")
                // Для преобразования в enum.
                .HasConversion<string>();
        });

        modelBuilder.Entity<DbBoard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__board__3213E83FDD2F00EE");

            entity.ToTable("board");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.ColorCode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("colorCode");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.DesignTypeId).HasColumnName("designTypeId");
            entity.Property(e => e.GeneralAccessLevelId).HasColumnName("generalAccessLevelId");
            entity.Property(e => e.ImageFileId).HasColumnName("imageFileId");
            entity.Property(e => e.IsPublic).HasColumnName("isPublic");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.DesignType).WithMany(p => p.Boards)
                .HasForeignKey(d => d.DesignTypeId)
                .HasConstraintName("FK__board__designTyp__4E88ABD4");

            entity.HasOne(d => d.GeneralAccessLevel).WithMany(p => p.Boards)
                .HasForeignKey(d => d.GeneralAccessLevelId)
                .HasConstraintName("FK__board__generalAc__4CA06362");

            entity.HasOne(d => d.ImageFile).WithMany(p => p.Boards)
                .HasForeignKey(d => d.ImageFileId)
                .HasConstraintName("FK__board__imageFile__4F7CD00D");

            entity.HasOne(d => d.User).WithMany(p => p.Boards)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__board__userId__4D94879B");
        });

        modelBuilder.Entity<DbBoardAccessLevelMap>(entity =>
        {
            entity.HasKey(e => new { e.BoardId, e.UserId }).HasName("PK__boardAcc__A13B6B5F36AC5E40");

            entity.ToTable("boardAccessLevelMap");

            entity.Property(e => e.BoardId).HasColumnName("boardId");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.AccessLevelId).HasColumnName("accessLevelId");

            entity.HasOne(d => d.AccessLevel).WithMany(p => p.BoardAccessLevelMaps)
                .HasForeignKey(d => d.AccessLevelId)
                .HasConstraintName("FK__boardAcce__acces__5441852A");

            entity.HasOne(d => d.Board).WithMany(p => p.BoardAccessLevelMaps)
                .HasForeignKey(d => d.BoardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__boardAcce__board__52593CB8");

            entity.HasOne(d => d.User).WithMany(p => p.BoardAccessLevelMaps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__boardAcce__userI__534D60F1");
        });

        modelBuilder.Entity<DbBoardViewHistoryMap>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.BoardId }).HasName("PK__boardVie__DF423056007BCD87");

            entity.ToTable("boardViewHistoryMap");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.BoardId).HasColumnName("boardId");
            entity.Property(e => e.LastViewedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("lastViewedAt");

            entity.HasOne(d => d.Board).WithMany(p => p.BoardViewHistoryMaps)
                .HasForeignKey(d => d.BoardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__boardView__board__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.BoardViewHistoryMaps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__boardView__userI__5812160E");
        });

        modelBuilder.Entity<DbCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__card__3213E83FEB566535");

            entity.ToTable("card");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CardListId).HasColumnName("cardListId");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageFileId).HasColumnName("imageFileId");
            entity.Property(e => e.NextCardId).HasColumnName("nextCardId");
            entity.Property(e => e.PrevCardId).HasColumnName("prevCardId");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.CardList).WithMany(p => p.Cards)
                .HasForeignKey(d => d.CardListId)
                .HasConstraintName("FK__card__cardListId__6383C8BA");

            entity.HasOne(d => d.ImageFile).WithMany(p => p.Cards)
                .HasForeignKey(d => d.ImageFileId)
                .HasConstraintName("FK__card__imageFileI__628FA481");

            entity.HasOne(d => d.NextCard).WithMany(p => p.InverseNextCard)
                .HasForeignKey(d => d.NextCardId)
                .HasConstraintName("FK__card__nextCardId__656C112C");

            entity.HasOne(d => d.PrevCard).WithMany(p => p.InversePrevCard)
                .HasForeignKey(d => d.PrevCardId)
                .HasConstraintName("FK__card__prevCardId__6477ECF3");
        });

        modelBuilder.Entity<DbCardAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cardAtta__3213E83F47926DAE");

            entity.ToTable("cardAttachment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CardId).HasColumnName("cardId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.FileId).HasColumnName("fileId");

            entity.HasOne(d => d.Card).WithMany(p => p.CardAttachments)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK__cardAttac__cardI__6A30C649");

            entity.HasOne(d => d.File).WithMany(p => p.CardAttachments)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cardAttac__fileI__6B24EA82");
        });

        modelBuilder.Entity<DbCardComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cardComm__3213E83F42918129");

            entity.ToTable("cardComment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CardId).HasColumnName("cardId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updatedAt");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Card).WithMany(p => p.CardComments)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK__cardComme__cardI__70DDC3D8");

            entity.HasOne(d => d.User).WithMany(p => p.CardComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__cardComme__userI__71D1E811");
        });

        modelBuilder.Entity<DbCardList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cardList__3213E83FD11097AB");

            entity.ToTable("cardList");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BoardId).HasColumnName("boardId");
            entity.Property(e => e.NextCardListId).HasColumnName("nextCardListId");
            entity.Property(e => e.PrevCardListId).HasColumnName("prevCardListId");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Board).WithMany(p => p.CardLists)
                .HasForeignKey(d => d.BoardId)
                .HasConstraintName("FK__cardList__boardI__5CD6CB2B");

            entity.HasOne(d => d.NextCardList).WithMany(p => p.InverseNextCardList)
                .HasForeignKey(d => d.NextCardListId)
                .HasConstraintName("FK__cardList__nextCa__5EBF139D");

            entity.HasOne(d => d.PrevCardList).WithMany(p => p.InversePrevCardList)
                .HasForeignKey(d => d.PrevCardListId)
                .HasConstraintName("FK__cardList__prevCa__5DCAEF64");
        });

        modelBuilder.Entity<DbDesign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__design__3213E83F4DFA767E");

            entity.ToTable("design");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type")
                // Для преобразования в enum.
                .HasConversion<string>();
        });

        modelBuilder.Entity<DbFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__file__3213E83F39AD2147");

            entity.ToTable("file");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("fileName");
            entity.Property(e => e.RelativePath)
                .HasMaxLength(255)
                .HasColumnName("relativePath");
        });

        modelBuilder.Entity<DbRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role__3213E83FBDEBAE23");

            entity.ToTable("role");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type")
                // Для преобразования в enum.
                .HasConversion<string>();
        });

        modelBuilder.Entity<DbUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83FF6C4923B");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "UQ__user__AB6E6164E27145AB").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("passwordHash");
            entity.Property(e => e.RoleId).HasColumnName("roleId");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__user__roleId__412EB0B6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
