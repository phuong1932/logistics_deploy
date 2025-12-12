-- Script tạo bảng Trackings dựa trên schema từ ảnh
-- SQL Server

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Trackings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Trackings] (
        [id] BIGINT NOT NULL IDENTITY(1,1),
        [store_id] BIGINT NOT NULL,
        [username] NVARCHAR(30) NOT NULL,
        [action] NVARCHAR(255) NOT NULL,
        [date_created] DATETIME NOT NULL,
        [ip] VARCHAR(20) NOT NULL,
        
        CONSTRAINT [PK_Trackings] PRIMARY KEY CLUSTERED ([id] ASC)
    );
    
    -- Tạo index cho store_id
    CREATE NONCLUSTERED INDEX [IX_Trackings_store_id] 
    ON [dbo].[Trackings] ([store_id] ASC);
    
    -- Tạo index cho username để tìm kiếm nhanh hơn
    CREATE NONCLUSTERED INDEX [IX_Trackings_username] 
    ON [dbo].[Trackings] ([username] ASC);
    
    -- Tạo index cho date_created để query theo thời gian
    CREATE NONCLUSTERED INDEX [IX_Trackings_date_created] 
    ON [dbo].[Trackings] ([date_created] DESC);
    
    PRINT 'Bảng Trackings đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng Trackings đã tồn tại!';
END
GO

