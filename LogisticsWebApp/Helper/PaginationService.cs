using System;
using System.Collections.Generic;
using System.Linq;

namespace LogisticsWebApp.Helper
{
    public class PaginationService<T>
    {
        public int CurrentPage { get; private set; } = 1;
        public int PageSize { get; private set; } = 10;
        public int TotalItems { get; private set; }
        public int TotalPages { get; private set; }
        public IEnumerable<T> PaginatedItems { get; private set; } = new List<T>();
        public IEnumerable<T> AllItems { get; private set; } = new List<T>();

        public PaginationService(int pageSize = 10)
        {
            PageSize = pageSize;
        }

        /// <summary>
        /// Thiết lập dữ liệu và cập nhật phân trang
        /// </summary>
        /// <param name="items">Danh sách tất cả items</param>
        /// <param name="currentPage">Trang hiện tại (mặc định là 1)</param>
        public void SetData(IEnumerable<T> items, int currentPage = 1)
        {
            AllItems = items ?? new List<T>();
            TotalItems = AllItems.Count();
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            
            // Đảm bảo currentPage nằm trong khoảng hợp lệ
            CurrentPage = Math.Max(1, Math.Min(currentPage, Math.Max(1, TotalPages)));
            
            UpdatePaginatedItems();
        }

        /// <summary>
        /// Cập nhật danh sách items đã phân trang
        /// </summary>
        private void UpdatePaginatedItems()
        {
            if (AllItems == null || !AllItems.Any())
            {
                PaginatedItems = new List<T>();
                return;
            }

            PaginatedItems = AllItems
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);
        }

        /// <summary>
        /// Chuyển đến trang đầu tiên
        /// </summary>
        public void GoToFirstPage()
        {
            if (TotalPages > 0)
            {
                CurrentPage = 1;
                UpdatePaginatedItems();
            }
        }

        /// <summary>
        /// Chuyển đến trang cuối cùng
        /// </summary>
        public void GoToLastPage()
        {
            if (TotalPages > 0)
            {
                CurrentPage = TotalPages;
                UpdatePaginatedItems();
            }
        }

        /// <summary>
        /// Chuyển đến trang tiếp theo
        /// </summary>
        public void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdatePaginatedItems();
            }
        }

        /// <summary>
        /// Chuyển đến trang trước đó
        /// </summary>
        public void GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdatePaginatedItems();
            }
        }

        /// <summary>
        /// Chuyển đến trang cụ thể
        /// </summary>
        /// <param name="page">Số trang</param>
        public void GoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                UpdatePaginatedItems();
            }
        }

        /// <summary>
        /// Lấy trang bắt đầu để hiển thị trong pagination
        /// </summary>
        /// <param name="maxVisiblePages">Số trang tối đa hiển thị (mặc định 6)</param>
        /// <returns>Trang bắt đầu</returns>
        public int GetStartPage(int maxVisiblePages = 6)
        {
            if (TotalPages <= maxVisiblePages)
                return 1;

            if (CurrentPage <= (maxVisiblePages / 2))
                return 1;

            if (CurrentPage >= TotalPages - (maxVisiblePages / 2))
                return TotalPages - maxVisiblePages + 1;

            return CurrentPage - (maxVisiblePages / 2);
        }

        /// <summary>
        /// Lấy trang kết thúc để hiển thị trong pagination
        /// </summary>
        /// <param name="maxVisiblePages">Số trang tối đa hiển thị (mặc định 6)</param>
        /// <returns>Trang kết thúc</returns>
        public int GetEndPage(int maxVisiblePages = 6)
        {
            if (TotalPages <= maxVisiblePages)
                return TotalPages;

            if (CurrentPage <= (maxVisiblePages / 2))
                return maxVisiblePages;

            if (CurrentPage >= TotalPages - (maxVisiblePages / 2))
                return TotalPages;

            return CurrentPage + (maxVisiblePages / 2);
        }

        /// <summary>
        /// Kiểm tra có thể chuyển đến trang trước không
        /// </summary>
        public bool CanGoToPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Kiểm tra có thể chuyển đến trang tiếp theo không
        /// </summary>
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Lấy thông tin phân trang
        /// </summary>
        public PaginationInfo GetPaginationInfo()
        {
            return new PaginationInfo
            {
                CurrentPage = CurrentPage,
                PageSize = PageSize,
                TotalItems = TotalItems,
                TotalPages = TotalPages,
                StartItem = TotalItems > 0 ? (CurrentPage - 1) * PageSize + 1 : 0,
                EndItem = Math.Min(CurrentPage * PageSize, TotalItems)
            };
        }

        /// <summary>
        /// Thay đổi kích thước trang
        /// </summary>
        /// <param name="newPageSize">Kích thước trang mới</param>
        public void ChangePageSize(int newPageSize)
        {
            if (newPageSize > 0)
            {
                PageSize = newPageSize;
                TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                CurrentPage = Math.Min(CurrentPage, Math.Max(1, TotalPages));
                UpdatePaginatedItems();
            }
        }

        /// <summary>
        /// Reset về trạng thái ban đầu
        /// </summary>
        public void Reset()
        {
            CurrentPage = 1;
            TotalItems = 0;
            TotalPages = 0;
            PaginatedItems = new List<T>();
            AllItems = new List<T>();
        }
    }

    /// <summary>
    /// Thông tin phân trang
    /// </summary>
    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int StartItem { get; set; }
        public int EndItem { get; set; }
    }
}
