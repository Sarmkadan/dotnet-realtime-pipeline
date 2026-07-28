[Test]
public class QueryServiceValidationTests
{
    [Test]
    public void Validate_Happy_PATH_DataPointRepository_Provided()
    {
        // Arrange
        var dataPointRepository = new Mock<DataPointRepository>();
        var metricsRepository = new Mock<MetricsRepository>();
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act
        var result = queryService.Validate();

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public void Validate_HAPPY_PATH_MetricsRepository_Provided()
    {
        // Arrange
        var dataPointRepository = new Mock<DataPointRepository>();
        var metricsRepository = new Mock<MetricsRepository>();
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act
        var result = queryService.Validate();

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public void Validate_NULL_DataPointRepository_ThrowsException()
    {
        // Arrange
        var dataPointRepository = (DataPointRepository) null;
        var metricsRepository = new Mock<MetricsRepository>();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new QueryService(dataPointRepository, metricsRepository));
    }

    [Test]
    public void Validate_NULL_MetricsRepository_ThrowsException()
    {
        // Arrange
        var dataPointRepository = new Mock<DataPointRepository>();
        var metricsRepository = (MetricsRepository) null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => new QueryService(dataPointRepository, metricsRepository));
    }

    [Test]
    public void IsValid_HAPPY_PATH_ReturnsTrue()
    {
        // Arrange
        var dataPointRepository = new Mock<DataPointRepository>();
        var metricsRepository = new Mock<MetricsRepository>();
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act
        var result = queryService.IsValid();

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void IsValid_NULL_DataPointRepository_ReturnsFalse()
    {
        // Arrange
        var dataPointRepository = (DataPointRepository) null;
        var metricsRepository = new Mock<MetricsRepository>();
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act
        var result = queryService.IsValid();

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void IsValid_NULL_MetricsRepository_ReturnsFalse()
    {
        // Arrange
        var dataPointRepository = new Mock<DataPointRepository>();
        var metricsRepository = (MetricsRepository) null;
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act
        var result = queryService.IsValid();

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void EnsureValid_HAPPY_PATH_NoException()
    {
        // Arrange
        var dataPointRepository = new Mock<DataPointRepository>();
        var metricsRepository = new Mock<MetricsRepository>();
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act
        queryService.EnsureValid();
    }

    [Test]
    public void EnsureValid_NULL_DataPointRepository_ThrowsException()
    {
        // Arrange
        var dataPointRepository = (DataPointRepository) null;
        var metricsRepository = new Mock<MetricsRepository>();
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => queryService.EnsureValid());
    }

    [Test]
    public void EnsureValid_NULL_MetricsRepository_ThrowsException()
    {
        // Arrange
        var dataPointRepository = new Mock<DataPointRepository>();
        var metricsRepository = (MetricsRepository) null;
        var queryService = new QueryService(dataPointRepository, metricsRepository);

        // Act and Assert
        Assert.Throws<ArgumentException>(() => queryService.EnsureValid());
    }
}
