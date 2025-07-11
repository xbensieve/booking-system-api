using AutoMapper;
using Booking.Repository.Interfaces;
using Booking.Repository.Models;
using Booking.Service.Implementations;
using Booking.Service.Models;
using Moq;
using Xunit;

namespace Booking.Tests.Services
{
    public class HotelServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HotelService _service;
        private readonly Mock<IGenericRepository<Hotel>> _mockHotelRepo;

        public HotelServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockHotelRepo = new Mock<IGenericRepository<Hotel>>();
            _mockUnitOfWork.Setup(u => u.Hotels).Returns(_mockHotelRepo.Object);
            _service = new HotelService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateHotelAsync_ReturnsSuccess_WhenValid()
        {
            // Arrange
            var request = new HotelRequest
            {
                Name = "Test Hotel",
                Address = "123 Street",
                City = "City",
                Country = "Country",
                Description = "Nice hotel"
            };

            var hotel = new Hotel { Id = 1, Name = request.Name, Address = request.Address, City = request.City, Country = request.Country, Description = request.Description };
            _mockMapper.Setup(m => m.Map<Hotel>(request)).Returns(hotel);
            _mockHotelRepo.Setup(r => r.AddAsync(hotel)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.CreateHotelAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Hotel created successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(hotel.Id, ((dynamic)result.Data).hotelId);
            _mockHotelRepo.Verify(r => r.AddAsync(hotel), Times.Once());
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task CreateHotelAsync_ReturnsFail_WhenSaveReturnsZero()
        {
            // Arrange
            var request = new HotelRequest
            {
                Name = "Test Hotel"
            };

            var hotel = new Hotel { Id = 1, Name = request.Name };
            _mockMapper.Setup(m => m.Map<Hotel>(request)).Returns(hotel);
            _mockHotelRepo.Setup(r => r.AddAsync(hotel)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _service.CreateHotelAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to create hotel. Please try again.", result.Message);
            _mockHotelRepo.Verify(r => r.AddAsync(hotel), Times.Once());
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task CreateHotelAsync_ThrowsArgumentNullException_WhenRequestIsNull()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateHotelAsync(null));
            Assert.Contains("Hotel request cannot be null", ex.Message);
        }

        [Fact]
        public async Task CreateHotelAsync_ReturnsFail_WhenExceptionThrown()
        {
            // Arrange
            var request = new HotelRequest { Name = "Crash" };
            var hotel = new Hotel { Id = 1, Name = request.Name };
            _mockMapper.Setup(m => m.Map<Hotel>(request)).Returns(hotel);
            _mockHotelRepo.Setup(r => r.AddAsync(hotel)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _service.CreateHotelAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An error occurred while creating the hotel.", result.Message);
            Assert.Contains("DB error", result.Message);
            _mockHotelRepo.Verify(r => r.AddAsync(hotel), Times.Once());
        }

        [Fact]
        public async Task GetHotelByIdAsync_ReturnsSuccess_WhenHotelExists()
        {
            // Arrange
            var hotel = new Hotel { Id = 1, Name = "Test Hotel" };
            var hotelResponse = new HotelResponse { Id = 1, Name = "Test Hotel" };
            _mockHotelRepo.Setup(r => r.Query()).Returns(new List<Hotel> { hotel }.AsQueryable());
            _mockMapper.Setup(m => m.Map<HotelResponse>(hotel)).Returns(hotelResponse);

            // Act
            var result = await _service.GetHotelByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Hotel retrieved successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(hotelResponse, result.Data);
            _mockMapper.Verify(m => m.Map<HotelResponse>(hotel), Times.Once());
        }

        [Fact]
        public async Task GetHotelByIdAsync_ReturnsFail_WhenHotelNotFound()
        {
            // Arrange
            _mockHotelRepo.Setup(r => r.Query()).Returns(new List<Hotel>().AsQueryable());

            // Act
            var result = await _service.GetHotelByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Hotel not found.", result.Message);
            Assert.Null(result.Data);
        }
    }
}