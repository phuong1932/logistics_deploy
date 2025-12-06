using logistic_web.infrastructure.Models;
using logistic_web.infrastructure.Repositories;
using logistic_web.infrastructure.Unitofwork;
using logistic_web.application.DTO;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.IO;
using System.Text.Json;

namespace logistic_web.application.Services
{
    public interface ICargoService
    {
        Task<IEnumerable<Cargolist>> GetAllCargoAsync();
        Task<Cargolist?> GetCargoByIdAsync(int id);
        Task<Cargolist> CreateCargoAsync(CargoDto cargoDto);
        Task<Cargolist?> UpdateCargoAsync(int id, CargoDto cargoDto);
        Task<bool> DeleteCargoAsync(int id);
        Task<IEnumerable<Cargolist>> SearchCargoByCustomerAsync(string customerName);
        Task<IEnumerable<Cargolist>> SearchCargoByNumAsync(string numOfCargo);
        Task<byte[]> ExportCargoDataToExcelAsync(DateTime? datebegin, DateTime? dateend);
        Task UpdateCargoFileBackgroundAsync(int id);
        
        // Lấy thống kê tổng hợp: số lượng + doanh thu
        Task<(int count, decimal totalRevenue)> GetMonthlyStatisticsAsync();
    }
    public class CargoService : ICargoService
    {
        private readonly ICargolistRepository _cargolistRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CargoService> _logger;

        public CargoService(
            ICargolistRepository cargolistRepository,
            IUnitOfWork unitOfWork,
            ILogger<CargoService> logger)
        {
            _cargolistRepository = cargolistRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Cargolist>> GetAllCargoAsync()
        {
            try
            {
                var cargos = await _cargolistRepository.GetAllAsync();
                _logger.LogInformation("Lấy danh sách {Count} cargo", cargos.Count());
                return cargos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cargo");
                throw;
            }
        }

        public async Task<Cargolist?> GetCargoByIdAsync(int id)
        {
            try
            {
                var cargo = await _cargolistRepository.GetByIdAsync(id);
                if (cargo != null)
                {
                    _logger.LogInformation("Lấy thông tin cargo ID: {CargoId}", id);
                }
                return cargo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cargo ID: {CargoId}", id);
                throw;
            }
        }

        public async Task<Cargolist> CreateCargoAsync(CargoDto cargoDto)
        {
            try
            {
                var cargo = new Cargolist
                {
                    CargoCode = cargoDto.CargoCode ?? $"CG{DateTime.Now:yyyyMMddHHmmss}",
                    CustomerCompanyName = cargoDto.CustomerCompanyName,
                    EmployeeCreate = cargoDto.EmployeeCreate,
                    CustomerPersonInCharge = cargoDto.CustomerPersonInCharge,
                    CustomerAddress = cargoDto.CustomerAddress,
                    ServiceType = cargoDto.ServiceType,
                    LicenseDate = DateTime.Now,
                    ExchangeDate = cargoDto.ExchangeDate,
                    EstimatedTotalAmount = cargoDto.EstimatedTotalAmount,
                    AdvanceMoney = cargoDto.AdvanceMoney,
                    ShippingFee = cargoDto.ShippingFee,
                    QuantityOfShipper = cargoDto.QuantityOfShipper,
                    CreatedAt = DateTime.Now
                };

                await _cargolistRepository.AddAsync(cargo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Tạo cargo mới: {CargoNum} cho khách hàng {CustomerName}",
                    cargo.CargoCode, cargo.CustomerCompanyName);

                // Tạo file cargo sau khi thêm thành công và lưu path
                string filePath = await CreateCargoFileAsync(cargo);
                if (!string.IsNullOrEmpty(filePath))
                {
                    cargo.FilePathJson = filePath;
                    _cargolistRepository.Update(cargo);
                    await _unitOfWork.SaveChangesAsync();
                }

                return cargo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo cargo mới");
                throw;
            }
        }

        public async Task<Cargolist?> UpdateCargoAsync(int id, CargoDto cargoDto)
        {
            try
            {
                var existingCargo = await _cargolistRepository.GetByIdAsync(id);
                if (existingCargo == null)
                {
                    return null;
                }

                // Cập nhật thông tin
                existingCargo.CargoCode = cargoDto.CargoCode ?? existingCargo.CargoCode;
                existingCargo.CustomerCompanyName = cargoDto.CustomerCompanyName;
                existingCargo.EmployeeCreate = cargoDto.EmployeeCreate;
                existingCargo.CustomerPersonInCharge = cargoDto.CustomerPersonInCharge;
                existingCargo.CustomerAddress = cargoDto.CustomerAddress;
                existingCargo.ServiceType = cargoDto.ServiceType;
                existingCargo.ExchangeDate = cargoDto.ExchangeDate;
                existingCargo.EstimatedTotalAmount = cargoDto.EstimatedTotalAmount;
                existingCargo.AdvanceMoney = cargoDto.AdvanceMoney;
                existingCargo.ShippingFee = cargoDto.ShippingFee;
                existingCargo.QuantityOfShipper = cargoDto.QuantityOfShipper;

                _cargolistRepository.Update(existingCargo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cập nhật cargo ID: {CargoId}", id);

                _ = Task.Run(async () => 
                {
                    try
                    {
                        await UpdateCargoFileAsync(existingCargo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi cập nhật file cargo background cho ID: {CargoId}", id);
                    }
                });

                return existingCargo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật cargo ID: {CargoId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCargoAsync(int id)
        {
            try
            {
                var cargo = await _cargolistRepository.GetByIdAsync(id);
                if (cargo == null)
                {
                    return false;
                }

                _cargolistRepository.Remove(cargo);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Xóa cargo ID: {CargoId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa cargo ID: {CargoId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Cargolist>> SearchCargoByCustomerAsync(string customerName)
        {
            try
            {
                var cargos = await _cargolistRepository.FindAsync(c => c.CustomerCompanyName.Contains(customerName));
                _logger.LogInformation("Tìm kiếm cargo theo tên khách hàng: {CustomerName}, tìm thấy {Count} kết quả",
                    customerName, cargos.Count());
                return cargos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm cargo theo tên khách hàng: {CustomerName}", customerName);
                throw;
            }
        }

        public async Task<IEnumerable<Cargolist>> SearchCargoByNumAsync(string cargoCode)
        {
            try
            {
                var cargos = await _cargolistRepository.FindAsync(c => c.CargoCode != null && c.CargoCode.Contains(cargoCode));
                _logger.LogInformation("Tìm kiếm cargo theo mã cargo: {CargoCode}, tìm thấy {Count} kết quả",
                    cargoCode, cargos.Count());
                return cargos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm cargo theo mã cargo: {CargoCode}", cargoCode);
                throw;
            }
        }


        public async Task<byte[]> ExportCargoDataToExcelAsync(DateTime? datebegin, DateTime? dateend)
        {
            try
            {
                IEnumerable<Cargolist> cargos;
                
                if (datebegin.HasValue && dateend.HasValue)
                {
                    // Xuất dữ liệu theo khoảng thời gian
                    cargos = await _cargolistRepository.FindAsync(c => c.CreatedAt >= datebegin && c.CreatedAt <= dateend);
                }
                else
                {
                    // Nếu không có ngày bắt đầu/kết thúc, xuất dữ liệu theo tháng hiện tại
                    var now = DateTime.Now;
                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                    cargos = await _cargolistRepository.FindAsync(c => c.CreatedAt >= firstDayOfMonth && c.CreatedAt <= lastDayOfMonth);
                }
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    // Sheet 1: Báo cáo Logistics
                    var worksheet = package.Workbook.Worksheets.Add("Master Data (All customers)");

                    // Tạo header phức tạp theo mẫu
                    CreateComplexHeader(worksheet);

                    // Thêm dữ liệu cargo
                    int dataRow = 5; // Bắt đầu từ hàng 10
                    foreach (var cargo in cargos)
                    {
                        AddCargoData(worksheet, cargo, dataRow);
                        dataRow++;
                    }

                    // Tạo phần báo cáo lợi nhuận/chi phí
                    // CreateProfitLossSection(worksheet, dataRow + 2);

                    // Định dạng và tự động điều chỉnh cột
                    FormatWorksheet(worksheet);

                    // Sheet 2: Phân tích hoạt động kinh doanh
                    var businessAnalysisWorksheet = package.Workbook.Worksheets.Add("Báo cáo");
                    CreateBusinessAnalysisSheet(businessAnalysisWorksheet, cargos);

                    var fileContent = package.GetAsByteArray();
                    return fileContent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất Excel cargo");
                throw;
            }
        }

        private void CreateComplexHeader(OfficeOpenXml.ExcelWorksheet worksheet)
        {
            // Header chính (hàng 3-6)
            // Thông tin chung (cột A-T, màu vàng)
            // [3, 1, 6, 1]: Merge các ô từ hàng 3 đến 6 ở cột 1 (A3:A6) để tạo header cho "Số lô hàng"
            worksheet.Column(1).Width = 12;  // Số lô hàng
            worksheet.Column(2).Width = 20;  // Tên Khách Hàng
            worksheet.Column(3).Width = 18;  // Tên Chủ Hàng
            worksheet.Column(4).Width = 15;  // Tên Nhân Viên
            worksheet.Column(5).Width = 12;  // Loại Hình
            worksheet.Column(6).Width = 20;  // Nơi Giao
            worksheet.Column(7).Width = 8;   // 20'
            worksheet.Column(8).Width = 8;   // 40'
            worksheet.Column(9).Width = 12;  // Hàng lẻ KGS
            worksheet.Column(10).Width = 12; // Hàng lẻ CBM
            worksheet.Column(11).Width = 12; // Hàng Air
            worksheet.Column(12).Width = 18; // Ngày nhận chứng từ
            worksheet.Column(13).Width = 15; // Ngày giao hàng
            worksheet.Column(14).Width = 15; // Tiền Ứng
            //thêm cột từ 15 đến 55
            for (int i = 15; i <= 56; i++)
            {
                worksheet.Column(i).Width = 18;
            }
            worksheet.Cells[1, 1, 4, 1].Merge = true;
            worksheet.Cells[1, 1].Value = "Số lô hàng";
            worksheet.Cells[1, 1, 4, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 1, 4, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            worksheet.Cells[1, 2, 4, 2].Merge = true;
            worksheet.Cells[1, 2].Value = "Tên Khách Hàng - TT";
            worksheet.Cells[1, 2, 4, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 2, 4, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            worksheet.Cells[1, 3, 4, 3].Merge = true;
            worksheet.Cells[1, 3].Value = "Tên Chủ Hàng";
            worksheet.Cells[1, 3, 4, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 3, 4, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            worksheet.Cells[1, 4, 4, 4].Merge = true;
            worksheet.Cells[1, 4].Value = "Tên Nhân Viên";
            worksheet.Cells[1, 4, 4, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 4, 4, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            worksheet.Cells[1, 5, 4, 5].Merge = true;
            worksheet.Cells[1, 5].Value = "Loại Hình";
            worksheet.Cells[1, 5, 4, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 5, 4, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            worksheet.Cells[1, 6, 4, 6].Merge = true;
            worksheet.Cells[1, 6].Value = "Nơi Giao";
            worksheet.Cells[1, 6, 4, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 6, 4, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            // Số lượng hàng hóa (cột N-R) - Header chính
            worksheet.Cells[1, 7, 2, 11].Merge = true;
            worksheet.Cells[1, 7].Value = "SỐ LƯỢNG HÀNG HÓA";
            worksheet.Cells[1, 7, 2, 11].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 7, 2, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            // Hàng SEA - Sub header
            worksheet.Cells[3, 7, 3, 10].Merge = true;
            worksheet.Cells[3, 7].Value = "Hàng SEA";
            worksheet.Cells[3, 7, 3, 10].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[3, 7, 3, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            // Chi tiết hàng SEA
            worksheet.Cells[4, 7].Value = "20'";
            worksheet.Cells[4, 8].Value = "40'";
            worksheet.Cells[4, 9, 4, 10].Merge = true;
            worksheet.Cells[4, 9].Value = "Hàng lẻ / LCL";
            worksheet.Cells[4, 9, 4, 10].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[4, 9, 4, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            // Hàng Air
            worksheet.Cells[3, 11, 4, 11].Merge = true;
            worksheet.Cells[3, 11].Value = "Hàng Air (CW)";
            worksheet.Cells[3, 11, 4, 11].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[3, 11, 4, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            // Ngày nhận chứng từ và ngày giao hàng
            worksheet.Cells[1, 12, 4, 12].Merge = true;
            worksheet.Cells[1, 12].Value = "Ngày nhận chứng từ";
            worksheet.Cells[1, 12, 4, 12].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 12, 4, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            worksheet.Cells[1, 13, 4, 13].Merge = true;
            worksheet.Cells[1, 13].Value = "Ngày giao hàng";
            worksheet.Cells[1, 13, 4, 13].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 13, 4, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#FFE699"));

            // Tiền ứng
            worksheet.Cells[1, 14, 4, 14].Merge = true;
            worksheet.Cells[1, 14].Value = "Tiền Ứng";
            worksheet.Cells[1, 14, 4, 14].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 14, 4, 14].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

            // Chi phí công ty Thiên Thành (cột V-AN, màu xanh nhạt)
            worksheet.Cells[1, 15, 2, 26].Merge = true;
            worksheet.Cells[1, 15].Value = "CÁC CHI PHÍ CỦA CTY THIÊN THANH";
            worksheet.Cells[1, 15, 2, 26].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 15, 2, 26].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            // Các loại chi phí
            string[] chiPhiLabels = {
                "ĐMức + LPHQ", "Phi Nâng/hạ", "Phi vận chuyển", "SỐ Lượng xe",
                "Xe Thien Thanh Xe thầu phụ", "Phi Bốc xếp / Xếp dỡ hàng hóa MMTB",
                "Cước Tàu", "Phí khác ...", "làm đại lý / Làm hàng tại cảng",
                "Chuyển khoản", "TOTAL", "Ghi Chú"
            };

            int col = 15;
            foreach (var label in chiPhiLabels)
            {
                worksheet.Cells[3, col, 4, col].Merge = true;
                worksheet.Cells[3, col].Value = label;
                worksheet.Cells[3, col, 4, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, col, 4, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                col++;
            }

            // Chi phí trả hộ cho khách hàng (cột AO-AY, màu đỏ)
            worksheet.Cells[1, 27, 2, 32].Merge = true;
            worksheet.Cells[1, 27].Value = "CÁC CHI PHÍ TRẢ HỘ CHO KHÁCH HÀNG";
            worksheet.Cells[1, 27, 2, 32].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 27, 2, 32].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);

            string[] chiPhiTraHoLabels = {
                "Phi D.O/ Handling", "THC", "Phi CFS / Ari port fee", "Phi nâng hạ",
                "Phi BILL , Phi lưu kho, lưu bãi,", "Thuế NK Cước Tàu"
            };

            col = 27;
            foreach (var label in chiPhiTraHoLabels)
            {
                worksheet.Cells[3, col, 4, col].Merge = true;
                worksheet.Cells[3, col].Value = label;
                worksheet.Cells[3, col, 4, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, col, 4, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);
                col++;
            }

            // Thêm các cột header mới theo cấu trúc trong ảnh
            // CHI PHÍ ĐẦU VÀO (cột 33-39, màu xanh nhạt)
            worksheet.Cells[1, 33, 2, 39].Merge = true;
            worksheet.Cells[1, 33].Value = "CHI PHÍ ĐẦU VÀO";
            worksheet.Cells[1, 33, 2, 39].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 33, 2, 39].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            string[] chiPhiDauVaoLabels = {
                "HẢI QUAN", "VẬN CHUYỂN", "CƯỚC QUỐC TẾ", "ĐẠI LÝ",
                "XẾP DỠ / LẮP ĐẶT", "KHÁC", "TOTAL"
            };

            col = 33;
            foreach (var label in chiPhiDauVaoLabels)
            {
                worksheet.Cells[3, col, 4, col].Merge = true;
                worksheet.Cells[3, col].Value = label;
                worksheet.Cells[3, col, 4, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, col, 4, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                col++;
            }

            // DOANH THU BÁN RA (cột 40-46, màu xanh lá nhạt)
            worksheet.Cells[1, 40, 2, 46].Merge = true;
            worksheet.Cells[1, 40].Value = "DOANH THU BÁN RA";
            worksheet.Cells[1, 40, 2, 46].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 40, 2, 46].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

            string[] doanhThuBanRaLabels = {
                "HẢI QUAN", "VẬN CHUYỂN", "CƯỚC QUỐC TẾ", "ĐẠI LÝ",
                "XẾP DỠ / LẮP ĐẶT", "KHÁC", "TOTAL"
            };

            col = 40;
            foreach (var label in doanhThuBanRaLabels)
            {
                worksheet.Cells[3, col, 4, col].Merge = true;
                worksheet.Cells[3, col].Value = label;
                worksheet.Cells[3, col, 4, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, col, 4, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                col++;
            }

            // LỢI NHUẬN/CHI PHÍ (cột 47-53, màu xanh nhạt)
            worksheet.Cells[1, 47, 2, 53].Merge = true;
            worksheet.Cells[1, 47].Value = "LỢI NHUẬN/CHI PHÍ";
            worksheet.Cells[1, 47, 2, 53].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 47, 2, 53].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            string[] loiNhuanChiPhiLabels = {
                "HẢI QUAN", "VẬN CHUYỂN", "CƯỚC QUỐC TẾ", "ĐẠI LÝ",
                "XẾP DỠ / LẮP ĐẶT", "KHÁC", "TOTAL"
            };

            col = 47;
            foreach (var label in loiNhuanChiPhiLabels)
            {
                worksheet.Cells[3, col, 4, col].Merge = true;
                worksheet.Cells[3, col].Value = label;
                worksheet.Cells[3, col, 4, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, col, 4, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                col++;
            }

            // LỢI NHUẬN (cột 54-55, màu xanh lá nhạt)
            worksheet.Cells[1, 54, 2, 55].Merge = true;
            worksheet.Cells[1, 54].Value = "LỢI NHUẬN";
            worksheet.Cells[1, 54, 2, 55].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 54, 2, 55].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

            // PROFIT / LOSS
            worksheet.Cells[3, 54, 4, 54].Merge = true;
            worksheet.Cells[3, 54].Value = "PROFIT / LOSS";
            worksheet.Cells[3, 54, 4, 54].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[3, 54, 4, 54].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

            // %
            worksheet.Cells[3, 55, 4, 55].Merge = true;
            worksheet.Cells[3, 55].Value = "%";
            worksheet.Cells[3, 55, 4, 55].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[3, 55, 4, 55].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);




        }

        private void AddCargoData(OfficeOpenXml.ExcelWorksheet worksheet, Cargolist cargo, int row)
        {
            worksheet.Cells[row, 1].Value = cargo.CargoCode;
            worksheet.Cells[row, 2].Value = cargo.CustomerCompanyName;
            worksheet.Cells[row, 3].Value = cargo.CustomerPersonInCharge;
            worksheet.Cells[row, 4].Value = cargo.EmployeeCreate;
            worksheet.Cells[row, 5].Value = cargo.ServiceType == 0 ? "XUAT" : cargo.ServiceType == 1 ? "NHAP" : "XUAT NHAP";
            worksheet.Cells[row, 6].Value = cargo.CustomerAddress;
            worksheet.Cells[row, 7].Value = "-"; // Hàng SEA 20'
            worksheet.Cells[row, 8].Value = "-"; // Hàng SEA 40'
            worksheet.Cells[row, 9].Value = "-"; // Hàng lẻ KGS
            worksheet.Cells[row, 10].Value = "-"; // Hàng lẻ CBM
            worksheet.Cells[row, 11].Value = cargo.ShippingFee?.ToString("N0"); // Hàng Air
            worksheet.Cells[row, 12].Value = cargo.LicenseDate?.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 13].Value = cargo.ExchangeDate?.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 14].Value = cargo.AdvanceMoney?.ToString("N0");

            // Các chi phí công ty (để trống hoặc có giá trị mặc định)
            for (int col = 15; col <= 26; col++)
            {
                worksheet.Cells[row, col].Value = "-";
            }

            // Các chi phí trả hộ (để trống hoặc có giá trị mặc định)
            for (int col = 27; col <= 32; col++)
            {
                worksheet.Cells[row, col].Value = "-";
            }

            // CHI PHÍ ĐẦU VÀO (cột 33-39)
            for (int col = 33; col <= 39; col++)
            {
                worksheet.Cells[row, col].Value = "-";
            }

            // DOANH THU BÁN RA (cột 40-46)
            for (int col = 40; col <= 46; col++)
            {
                worksheet.Cells[row, col].Value = "-";
            }

            // LỢI NHUẬN/CHI PHÍ (cột 47-53)
            for (int col = 47; col <= 53; col++)
            {
                worksheet.Cells[row, col].Value = "-";
            }

            // LỢI NHUẬN (cột 54-55)
            worksheet.Cells[row, 54].Value = "-"; // PROFIT / LOSS
            worksheet.Cells[row, 55].Value = "-"; // %
        }


        private void FormatWorksheet(OfficeOpenXml.ExcelWorksheet worksheet)
        {
            // Đặt border cho tất cả các cell
            var allCells = worksheet.Cells[worksheet.Dimension.Address];
            allCells.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            // Căn giữa text cho header
            var headerRange = worksheet.Cells[1, 1, 4, 55];
            headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Khóa 2 cột đầu tiên (A và B) và 4 hàng đầu tiên (header) khi cuộn
            worksheet.View.FreezePanes(1, 3);

        }

        private void CreateBusinessAnalysisSheet(OfficeOpenXml.ExcelWorksheet worksheet, IEnumerable<Cargolist> cargos)
        {
            // Tiêu đề chính
            worksheet.Cells[1, 1, 1, 15].Merge = true;
            worksheet.Cells[1, 1].Value = "PHÂN TÍCH HOẠT ĐỘNG KINH DOANH";
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, 1, 15].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 1, 1, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

            // Tạo 5 bảng phân tích
            CreateBusinessAnalysisTables(worksheet, cargos);

            // Định dạng worksheet
            FormatBusinessAnalysisWorksheet(worksheet);
        }

        private void CreateBusinessAnalysisTables(OfficeOpenXml.ExcelWorksheet worksheet, IEnumerable<Cargolist> cargos)
        {
            int currentRow = 3;

            // Bảng 1: DOANH THU (REVENUE)
            currentRow = CreateRevenueTable(worksheet, currentRow, cargos);

            // Bảng 2: CHI PHÍ (COST)
            currentRow = CreateCostTable(worksheet, currentRow, cargos);

            // Bảng 3: LỢI NHUẬN (PROFIT)
            currentRow = CreateProfitTable(worksheet, currentRow, cargos);

            // Bảng 4: TỈ SUẤT LỢI NHUẬN (PROFIT MARGIN RATIO)
            currentRow = CreateProfitMarginTable(worksheet, currentRow, cargos);

            // Bảng 5: GROSS MARGIN for each segment compared to Total Gross profit (Ratio)
            currentRow = CreateGrossMarginTable(worksheet, currentRow, cargos);
        }

        private int CreateRevenueTable(OfficeOpenXml.ExcelWorksheet worksheet, int startRow, IEnumerable<Cargolist> cargos)
        {
            // Tiêu đề bảng
            worksheet.Cells[startRow, 1, startRow, 15].Merge = true;
            worksheet.Cells[startRow, 1].Value = "DOANH THU";
            worksheet.Cells[startRow, 1].Style.Font.Bold = true;
            worksheet.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            startRow++;

            // Header columns
            string[] headers = { "STT", "LOẠI HÌNH DỊCH VỤ", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "TỔNG CỘNG" };

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cells[startRow, col].Value = headers[col - 1];
                worksheet.Cells[startRow, col].Style.Font.Bold = true;
                worksheet.Cells[startRow, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            startRow++;

            // Dữ liệu các loại dịch vụ
            string[] serviceTypes = { "HẢI QUAN", "VẬN CHUYỂN", "CƯỚC QUỐC TẾ", "ĐẠI LÝ",
                                    "LẮP ĐẶT / XẾP DỠ", "DOANH THU KHÁC" };

            for (int i = 0; i < serviceTypes.Length; i++)
            {
                worksheet.Cells[startRow, 1].Value = i + 1; // STT
                worksheet.Cells[startRow, 2].Value = serviceTypes[i]; // Loại hình dịch vụ

                // Tính doanh thu cho tháng 1 (Jan) dựa trên dữ liệu cargo
                decimal janRevenue = CalculateRevenueForService(cargos, serviceTypes[i], 1);
                worksheet.Cells[startRow, 3].Value = janRevenue > 0 ? janRevenue.ToString("N0") : "";

                // Các tháng khác để trống
                for (int col = 4; col <= 14; col++)
                {
                    worksheet.Cells[startRow, col].Value = "";
                }

                // Tổng cộng
                worksheet.Cells[startRow, 15].Value = janRevenue > 0 ? janRevenue.ToString("N0") : "";

                startRow++;
            }

            // Dòng TOTAL
            worksheet.Cells[startRow, 1].Value = "";
            worksheet.Cells[startRow, 2].Value = "TOTAL";
            worksheet.Cells[startRow, 2].Style.Font.Bold = true;
            worksheet.Cells[startRow, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[startRow, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

            decimal totalRevenue = CalculateTotalRevenue(cargos, 1);
            worksheet.Cells[startRow, 3].Value = totalRevenue > 0 ? totalRevenue.ToString("N0") : "";
            worksheet.Cells[startRow, 15].Value = totalRevenue > 0 ? totalRevenue.ToString("N0") : "";

            // Tô màu vàng cho dòng TOTAL
            for (int col = 1; col <= 15; col++)
            {
                worksheet.Cells[startRow, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[startRow, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }

            return startRow + 2; // Trả về hàng tiếp theo với khoảng cách
        }

        private int CreateCostTable(OfficeOpenXml.ExcelWorksheet worksheet, int startRow, IEnumerable<Cargolist> cargos)
        {
            // Tiêu đề bảng
            worksheet.Cells[startRow, 1, startRow, 15].Merge = true;
            worksheet.Cells[startRow, 1].Value = "CHI PHÍ";
            worksheet.Cells[startRow, 1].Style.Font.Bold = true;
            worksheet.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            startRow++;

            // Header columns
            string[] headers = { "STT", "LOẠI HÌNH DỊCH VỤ", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "TOTAL" };

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cells[startRow, col].Value = headers[col - 1];
                worksheet.Cells[startRow, col].Style.Font.Bold = true;
                worksheet.Cells[startRow, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            startRow++;

            // Dữ liệu các loại dịch vụ
            string[] serviceTypes = { "HẢI QUAN", "VẬN CHUYỂN", "CƯỚC QUỐC TẾ", "ĐẠI LÝ",
                                    "LẮP ĐẶT / XẾP DỠ", "DOANH THU KHÁC" };

            for (int i = 0; i < serviceTypes.Length; i++)
            {
                worksheet.Cells[startRow, 1].Value = i + 1; // STT
                worksheet.Cells[startRow, 2].Value = serviceTypes[i]; // Loại hình dịch vụ

                // Tính chi phí cho tháng 1 (Jan) dựa trên dữ liệu cargo
                decimal janCost = CalculateCostForService(cargos, serviceTypes[i], 1);
                worksheet.Cells[startRow, 3].Value = janCost > 0 ? janCost.ToString("N0") : "";

                // Các tháng khác để trống
                for (int col = 4; col <= 14; col++)
                {
                    worksheet.Cells[startRow, col].Value = "";
                }

                // Total
                worksheet.Cells[startRow, 15].Value = janCost > 0 ? janCost.ToString("N0") : "";

                startRow++;
            }

            // Dòng TOTAL
            worksheet.Cells[startRow, 1].Value = "";
            worksheet.Cells[startRow, 2].Value = "TOTAL";
            worksheet.Cells[startRow, 2].Style.Font.Bold = true;

            decimal totalCost = CalculateTotalCost(cargos, 1);
            worksheet.Cells[startRow, 3].Value = totalCost > 0 ? totalCost.ToString("N0") : "";
            worksheet.Cells[startRow, 15].Value = totalCost > 0 ? totalCost.ToString("N0") : "";

            // Tô màu vàng cho dòng TOTAL
            for (int col = 1; col <= 15; col++)
            {
                worksheet.Cells[startRow, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[startRow, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }

            return startRow + 2; // Trả về hàng tiếp theo với khoảng cách
        }

        private int CreateProfitTable(OfficeOpenXml.ExcelWorksheet worksheet, int startRow, IEnumerable<Cargolist> cargos)
        {
            // Tiêu đề bảng
            worksheet.Cells[startRow, 1, startRow, 15].Merge = true;
            worksheet.Cells[startRow, 1].Value = "LỢI NHUẬN";
            worksheet.Cells[startRow, 1].Style.Font.Bold = true;
            worksheet.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            startRow++;

            // Header columns
            string[] headers = { "STT", "LOẠI HÌNH DỊCH VỤ", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "TOTAL" };

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cells[startRow, col].Value = headers[col - 1];
                worksheet.Cells[startRow, col].Style.Font.Bold = true;
                worksheet.Cells[startRow, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            startRow++;

            // Dữ liệu các loại dịch vụ (khác với bảng doanh thu)
            string[] serviceTypes = { "VẬN TẢI", "CƯỚC (SEA + AIR)", "LẮP ĐẶT", "HẢI QUAN",
                                    "ĐẠI LÝ", "THU TUC HAI QUAN" };

            for (int i = 0; i < serviceTypes.Length; i++)
            {
                worksheet.Cells[startRow, 1].Value = i + 1; // STT
                worksheet.Cells[startRow, 2].Value = serviceTypes[i]; // Loại hình dịch vụ

                // Tính lợi nhuận cho tháng 1 (Jan) dựa trên dữ liệu cargo
                decimal janProfit = CalculateProfitForService(cargos, serviceTypes[i], 1);
                worksheet.Cells[startRow, 3].Value = janProfit > 0 ? janProfit.ToString("N0") : "";

                // Các tháng khác để trống
                for (int col = 4; col <= 14; col++)
                {
                    worksheet.Cells[startRow, col].Value = "";
                }

                // Total
                worksheet.Cells[startRow, 15].Value = janProfit > 0 ? janProfit.ToString("N0") : "";

                startRow++;
            }

            // Dòng TOTAL
            worksheet.Cells[startRow, 1].Value = "";
            worksheet.Cells[startRow, 2].Value = "TOTAL";
            worksheet.Cells[startRow, 2].Style.Font.Bold = true;

            decimal totalProfit = CalculateTotalProfit(cargos, 1);
            worksheet.Cells[startRow, 3].Value = totalProfit > 0 ? totalProfit.ToString("N0") : "";
            worksheet.Cells[startRow, 15].Value = totalProfit > 0 ? totalProfit.ToString("N0") : "";

            // Tô màu vàng cho dòng TOTAL
            for (int col = 1; col <= 15; col++)
            {
                worksheet.Cells[startRow, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[startRow, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }

            return startRow + 2; // Trả về hàng tiếp theo với khoảng cách
        }

        private int CreateProfitMarginTable(OfficeOpenXml.ExcelWorksheet worksheet, int startRow, IEnumerable<Cargolist> cargos)
        {
            // Tiêu đề bảng
            worksheet.Cells[startRow, 1, startRow, 15].Merge = true;
            worksheet.Cells[startRow, 1].Value = "TỈ SUẤT LỢI NHUẬN";
            worksheet.Cells[startRow, 1].Style.Font.Bold = true;
            worksheet.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            startRow++;

            // Header columns
            string[] headers = { "STT", "LOẠI HÌNH DỊCH VỤ", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Total" };

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cells[startRow, col].Value = headers[col - 1];
                worksheet.Cells[startRow, col].Style.Font.Bold = true;
                worksheet.Cells[startRow, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            startRow++;

            // Dữ liệu các loại dịch vụ
            string[] serviceTypes = { "HẢI QUAN", "VẬN CHUYỂN", "CƯỚC QUỐC TẾ", "ĐẠI LÝ",
                                    "LẮP ĐẶT / XẾP DỠ", "DOANH THU KHÁC" };

            for (int i = 0; i < serviceTypes.Length; i++)
            {
                worksheet.Cells[startRow, 1].Value = i + 1; // STT
                worksheet.Cells[startRow, 2].Value = serviceTypes[i]; // Loại hình dịch vụ

                // Tính tỉ suất lợi nhuận cho tháng 1 (Jan)
                decimal profitMargin = CalculateProfitMarginForService(cargos, serviceTypes[i], 1);
                if (profitMargin > 0)
                {
                    worksheet.Cells[startRow, 3].Value = $"{profitMargin:F1}%";
                }
                else
                {
                    worksheet.Cells[startRow, 3].Value = "#DIV/0!";
                }

                // Các tháng khác để trống hoặc #DIV/0!
                for (int col = 4; col <= 14; col++)
                {
                    worksheet.Cells[startRow, col].Value = "";
                }

                // Total
                if (profitMargin > 0)
                {
                    worksheet.Cells[startRow, 15].Value = $"{profitMargin:F1}%";
                }
                else
                {
                    worksheet.Cells[startRow, 15].Value = "#DIV/0!";
                }

                startRow++;
            }

            // Dòng TOTAL
            worksheet.Cells[startRow, 1].Value = "";
            worksheet.Cells[startRow, 2].Value = "TOTAL";
            worksheet.Cells[startRow, 2].Style.Font.Bold = true;

            decimal totalProfitMargin = CalculateTotalProfitMargin(cargos, 1);
            if (totalProfitMargin > 0)
            {
                worksheet.Cells[startRow, 3].Value = $"{totalProfitMargin:F1}%";
                worksheet.Cells[startRow, 15].Value = $"{totalProfitMargin:F1}%";
            }
            else
            {
                worksheet.Cells[startRow, 3].Value = "#DIV/0!";
                worksheet.Cells[startRow, 15].Value = "#DIV/0!";
            }

            // Tô màu vàng cho dòng TOTAL
            for (int col = 1; col <= 15; col++)
            {
                worksheet.Cells[startRow, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[startRow, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }

            return startRow + 2; // Trả về hàng tiếp theo với khoảng cách
        }

        private int CreateGrossMarginTable(OfficeOpenXml.ExcelWorksheet worksheet, int startRow, IEnumerable<Cargolist> cargos)
        {
            // Tiêu đề bảng
            worksheet.Cells[startRow, 1, startRow, 15].Merge = true;
            worksheet.Cells[startRow, 1].Value = "GROSS MARGIN for each segment compared to Total Gross profit (Ratio)";
            worksheet.Cells[startRow, 1].Style.Font.Bold = true;
            worksheet.Cells[startRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[startRow, 1, startRow, 15].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            startRow++;

            // Header columns
            string[] headers = { "STT", "LOẠI HÌNH DỊCH VỤ", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                               "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "AVG" };

            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cells[startRow, col].Value = headers[col - 1];
                worksheet.Cells[startRow, col].Style.Font.Bold = true;
                worksheet.Cells[startRow, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }
            startRow++;

            // Dữ liệu các loại dịch vụ
            string[] serviceTypes = { "HẢI QUAN", "VẬN CHUYỂN", "CƯỚC QUỐC TẾ", "ĐẠI LÝ",
                                    "LẮP ĐẶT / XẾP DỠ", "DOANH THU KHÁC" };

            for (int i = 0; i < serviceTypes.Length; i++)
            {
                worksheet.Cells[startRow, 1].Value = i + 1; // STT
                worksheet.Cells[startRow, 2].Value = serviceTypes[i]; // Loại hình dịch vụ

                // Tính gross margin cho tháng 1 (Jan)
                decimal grossMargin = CalculateGrossMarginForService(cargos, serviceTypes[i], 1);
                worksheet.Cells[startRow, 3].Value = grossMargin > 0 ? $"{grossMargin:F1}%" : "0.0%";

                // Các tháng khác để 0.0%
                for (int col = 4; col <= 14; col++)
                {
                    worksheet.Cells[startRow, col].Value = "0.0%";
                }

                // AVG
                decimal avgMargin = grossMargin / 12; // Chia cho 12 tháng
                worksheet.Cells[startRow, 15].Value = $"{avgMargin:F1}%";

                startRow++;
            }

            // Dòng TOTAL
            worksheet.Cells[startRow, 1].Value = "";
            worksheet.Cells[startRow, 2].Value = "TOTAL";
            worksheet.Cells[startRow, 2].Style.Font.Bold = true;

            decimal totalGrossMargin = CalculateTotalGrossMargin(cargos, 1);
            worksheet.Cells[startRow, 3].Value = totalGrossMargin > 0 ? $"{totalGrossMargin:F1}%" : "0.0%";

            // Các tháng khác để 0.0%
            for (int col = 4; col <= 14; col++)
            {
                worksheet.Cells[startRow, col].Value = "0.0%";
            }

            // AVG
            decimal totalAvgMargin = totalGrossMargin / 12;
            worksheet.Cells[startRow, 15].Value = $"{totalAvgMargin:F1}%";

            // Tô màu vàng cho dòng TOTAL
            for (int col = 1; col <= 15; col++)
            {
                worksheet.Cells[startRow, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[startRow, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }

            return startRow + 2; // Trả về hàng tiếp theo với khoảng cách
        }

        private void FormatBusinessAnalysisWorksheet(OfficeOpenXml.ExcelWorksheet worksheet)
        {
            // Đặt border cho tất cả các cell
            var allCells = worksheet.Cells[worksheet.Dimension.Address];
            allCells.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            allCells.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            // Căn giữa text cho header
            var headerRange = worksheet.Cells[1, 1, 1, 15];
            headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Đặt độ rộng cột
            worksheet.Column(1).Width = 5;  // STT
            worksheet.Column(2).Width = 25; // LOẠI HÌNH DỊCH VỤ
            for (int col = 3; col <= 15; col++)
            {
                worksheet.Column(col).Width = 12; // Các cột tháng và tổng
            }
        }

        // Các method tính toán dữ liệu
        private decimal CalculateRevenueForService(IEnumerable<Cargolist> cargos, string serviceType, int month)
        {
            // Logic tính doanh thu dựa trên loại dịch vụ và tháng
            // Ví dụ: tính dựa trên EstimatedTotalAmount và ServiceType
            var relevantCargos = cargos.Where(c =>
                c.ExchangeDate?.Month == month &&
                GetServiceTypeName(c.ServiceType) == serviceType);

            return relevantCargos.Sum(c => c.EstimatedTotalAmount ?? 0);
        }

        private decimal CalculateCostForService(IEnumerable<Cargolist> cargos, string serviceType, int month)
        {
            // Logic tính chi phí dựa trên loại dịch vụ và tháng
            // Ví dụ: tính dựa trên AdvanceMoney và ShippingFee
            var relevantCargos = cargos.Where(c =>
                c.ExchangeDate?.Month == month &&
                GetServiceTypeName(c.ServiceType) == serviceType);

            return relevantCargos.Sum(c => (c.AdvanceMoney ?? 0) + (c.ShippingFee ?? 0));
        }

        private decimal CalculateProfitForService(IEnumerable<Cargolist> cargos, string serviceType, int month)
        {
            // Logic tính lợi nhuận = doanh thu - chi phí
            decimal revenue = CalculateRevenueForService(cargos, serviceType, month);
            decimal cost = CalculateCostForService(cargos, serviceType, month);
            return revenue - cost;
        }

        private decimal CalculateProfitMarginForService(IEnumerable<Cargolist> cargos, string serviceType, int month)
        {
            decimal revenue = CalculateRevenueForService(cargos, serviceType, month);
            decimal profit = CalculateProfitForService(cargos, serviceType, month);

            if (revenue > 0)
                return (profit / revenue) * 100;
            return 0;
        }

        private decimal CalculateGrossMarginForService(IEnumerable<Cargolist> cargos, string serviceType, int month)
        {
            decimal totalProfit = CalculateTotalProfit(cargos, month);
            decimal serviceProfit = CalculateProfitForService(cargos, serviceType, month);

            if (totalProfit > 0)
                return (serviceProfit / totalProfit) * 100;
            return 0;
        }

        private decimal CalculateTotalRevenue(IEnumerable<Cargolist> cargos, int month)
        {
            var relevantCargos = cargos.Where(c => c.ExchangeDate?.Month == month);
            return relevantCargos.Sum(c => c.EstimatedTotalAmount ?? 0);
        }

        private decimal CalculateTotalCost(IEnumerable<Cargolist> cargos, int month)
        {
            var relevantCargos = cargos.Where(c => c.ExchangeDate?.Month == month);
            return relevantCargos.Sum(c => (c.AdvanceMoney ?? 0) + (c.ShippingFee ?? 0));
        }

        private decimal CalculateTotalProfit(IEnumerable<Cargolist> cargos, int month)
        {
            return CalculateTotalRevenue(cargos, month) - CalculateTotalCost(cargos, month);
        }

        private decimal CalculateTotalProfitMargin(IEnumerable<Cargolist> cargos, int month)
        {
            decimal totalRevenue = CalculateTotalRevenue(cargos, month);
            decimal totalProfit = CalculateTotalProfit(cargos, month);

            if (totalRevenue > 0)
                return (totalProfit / totalRevenue) * 100;
            return 0;
        }

        private decimal CalculateTotalGrossMargin(IEnumerable<Cargolist> cargos, int month)
        {
            decimal totalProfit = CalculateTotalProfit(cargos, month);
            return totalProfit > 0 ? 100 : 0; // 100% nếu có lợi nhuận, 0% nếu không
        }

        private string GetServiceTypeName(byte? serviceType)
        {
            return serviceType switch
            {
                0 => "HẢI QUAN",
                1 => "VẬN CHUYỂN",
                2 => "CƯỚC QUỐC TẾ",
                _ => "DOANH THU KHÁC"
            };
        }

        /// <summary>
        /// Tạo file cargo sau khi thêm cargo thành công
        /// </summary>
        /// <param name="cargo">Thông tin cargo vừa được tạo</param>
        /// <returns>Đường dẫn file đã tạo</returns>
        private async Task<string> CreateCargoFileAsync(Cargolist cargo)
        {
            try
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // Tạo đường dẫn thư mục tuyệt đối tới /app/wwwroot/cargo/cargo_list/ngày hiện tại
                string basePath = Path.Combine("wwwroot", "cargo", "cargo_list", currentDate);
            
                // DEBUG: Log thư mục hiện tại và thư mục basePath để kiểm tra
                string tempDebugPath = Path.GetFullPath(basePath);
                _logger.LogInformation("DEBUG - Current Directory: {Dir}", Directory.GetCurrentDirectory());
                _logger.LogInformation("DEBUG - Calculated basePath: {BasePath}", tempDebugPath);

                // Log đường dẫn để debug
                _logger.LogInformation("Đường dẫn thư mục: {Path}", Path.GetFullPath(basePath));

                // Kiểm tra và tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    _logger.LogInformation("Đã tạo thư mục: {Path}", Path.GetFullPath(basePath));
                }
                else
                {
                    _logger.LogInformation("Thư mục đã tồn tại: {Path}", Path.GetFullPath(basePath));
                }

                // Tạo tên file với mã cargo và ngày tạo
                string createDate = cargo.CreatedAt?.ToString("yyyyMMdd") ?? DateTime.Now.ToString("yyyyMMdd");
                string fileName = $"{cargo.CargoCode}_{createDate}.json";
                string filePath = Path.Combine(basePath, fileName);

                // Tạo nội dung file JSON
                string fileContent = GenerateCargoJsonContent(cargo);

                // Ghi file
                try
                {
                    await File.WriteAllTextAsync(filePath, fileContent);
                    _logger.LogInformation("DEBUG - Ghi file thành công: {FilePath}", Path.GetFullPath(filePath));
                }
                catch (Exception fileEx)
                {
                    _logger.LogError(fileEx, "DEBUG - Lỗi khi ghi file cargo ở đường dẫn: {FilePath}", Path.GetFullPath(filePath));
                    throw; // Nếu muốn không throw thì có thể bỏ dòng này, hoặc trả về chuỗi lỗi tuỳ workflow
                }

                _logger.LogInformation("Đã tạo file cargo: {FilePath}", Path.GetFullPath(filePath));
                return Path.GetFullPath(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo file cargo cho ID: {CargoId}", cargo.Id);
                // Không throw exception để không ảnh hưởng đến việc tạo cargo
                return string.Empty;
            }
        }

        /// <summary>
        /// Tạo nội dung file cargo dạng JSON
        /// </summary>
        /// <param name="cargo">Thông tin cargo</param>
        /// <returns>Nội dung file dạng JSON</returns>
        private string GenerateCargoJsonContent(Cargolist cargo)
        {
            var cargoData = new
            {
                metadata = new
                {
                    fileType = "Cargo Information",
                    version = "1.0",
                    createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    description = "File này được tạo tự động khi thêm cargo mới"
                },
                cargoInfo = new
                {
                    id = cargo.Id,
                    cargoCode = cargo.CargoCode,
                    customerCompanyName = cargo.CustomerCompanyName,
                    customerPersonInCharge = cargo.CustomerPersonInCharge,
                    employeeCreate = cargo.EmployeeCreate,
                    customerAddress = cargo.CustomerAddress,
                    serviceType = new
                    {
                        code = cargo.ServiceType,
                        name = GetServiceTypeName(cargo.ServiceType)
                    },
                    dates = new
                    {
                        licenseDate = cargo.LicenseDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        exchangeDate = cargo.ExchangeDate?.ToString("yyyy-MM-dd"),
                        createdAt = cargo.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    },
                    financial = new
                    {
                        estimatedTotalAmount = cargo.EstimatedTotalAmount,
                        advanceMoney = cargo.AdvanceMoney,
                        shippingFee = cargo.ShippingFee,
                        estimatedTotalAmountFormatted = cargo.EstimatedTotalAmount?.ToString("N0") + " VND",
                        advanceMoneyFormatted = cargo.AdvanceMoney?.ToString("N0") + " VND",
                        shippingFeeFormatted = cargo.ShippingFee?.ToString("N0") + " VND"
                    },
                    logistics = new
                    {
                        quantityOfShipper = cargo.QuantityOfShipper
                    }
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.Serialize(cargoData, options);
        }

        /// <summary>
        /// Cập nhật file cargo trong background (gọi từ API riêng)
        /// </summary>
        /// <param name="id">ID cargo</param>
        public async Task UpdateCargoFileBackgroundAsync(int id)
        {
            try
            {
                var cargo = await _cargolistRepository.GetByIdAsync(id);
                if (cargo != null)
                {
                    await UpdateCargoFileAsync(cargo);
                    _logger.LogInformation("Background task: Đã cập nhật file cargo ID: {CargoId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background task: Lỗi khi cập nhật file cargo ID: {CargoId}", id);
            }
        }

        /// <summary>
        /// Cập nhật file cargo sau khi cập nhật cargo thành công
        /// </summary>
        /// <param name="cargo">Thông tin cargo đã được cập nhật</param>
        private async Task UpdateCargoFileAsync(Cargolist cargo)
        {
        
            try
            {
                string tempFilePath = cargo.FilePathJson + ".tmp";
                string fileContent = GenerateCargoJsonContent(cargo);

                // Ghi vào file tạm
                await File.WriteAllTextAsync(tempFilePath, fileContent);

                // Atomic move: temp → final (instant)
                if (File.Exists(cargo.FilePathJson))
                {
                    File.Delete(cargo.FilePathJson);
                }
                File.Move(tempFilePath, cargo.FilePathJson);

                _logger.LogInformation("Đã cập nhật file cargo: {FilePath}", cargo.FilePathJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật file cargo cho ID: {CargoId}", cargo.Id);
            }
        }

     
        /// <summary>
        /// Lấy thống kê tổng hợp trong tháng hiện tại
        /// Bao gồm: Số lượng đơn hàng + Tổng doanh thu
        /// </summary>
        /// <returns>Tuple chứa (số lượng đơn hàng, tổng doanh thu)</returns>
        public async Task<(int count, decimal totalRevenue)> GetMonthlyStatisticsAsync()
        {
            try
            {
                var statistics = await _cargolistRepository.GetMonthlyStatisticsAsync();
                _logger.LogInformation("Thống kê tháng hiện tại: {Count} đơn hàng, Doanh thu: {Revenue:N0} VND", 
                    statistics.count, statistics.totalRevenue);
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thống kê tháng hiện tại");
                throw;
            }
        }
    }
}
