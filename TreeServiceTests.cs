using AdvertisingAreas.Server.Models;
using AdvertisingAreas.Server.Services;

namespace AdvertisingAreas.Server.Tests
{
    public class TreeServiceTests
    {
        private readonly TreeService _treeService;
        private readonly Tree _tree;

        public TreeServiceTests()
        {
            _treeService = new TreeService();
            _tree = new Tree();
        }

        [Fact]
        public async Task FileConverter_ShouldReturnCorrectPlatformData()
        {
            // Arrange
            var testFile = "test.txt";
            var input = "Yandex.Direct:/ru\nGoogle Ads:/com\nVKontakte:/ru,/ua,/by,/kz\nFacebook Ads:/com,/us,/gb,/ca";
            File.WriteAllText(testFile, input);

            // Act
            var result = await _treeService.FileConverter(testFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("Yandex.Direct", result[0].Name);
            Assert.Equal("ru", result[0].Paths);
            Assert.Equal("Google Ads", result[1].Name);
            Assert.Equal("com", result[1].Paths);
            Assert.Equal("VKontakte", result[2].Name);
            Assert.Equal("ru", result[2].Paths);
            Assert.Equal("ua", result[3].Paths);
            Assert.Equal("by", result[4].Paths);
            Assert.Equal("kz", result[5].Paths);

            File.Delete(testFile);
        }

        [Fact]
        public void CreateTree_ShouldCreateTreeCorrectly()
        {
            // Arrange
            var platforms = new List<NodeInputData>
            {
                new NodeInputData("Yandex.Direct", "ru"),
                new NodeInputData("Google Ads", "com"),
                new NodeInputData("VKontakte", "ru"),
                new NodeInputData("VKontakte", "ua")
            };

            // Act
            _treeService.CreateTree(platforms, _tree);

            // Assert
            Assert.Equal(4, _tree.Platforms.Count);  // Должно быть 4 узла
            Assert.Contains(_tree.Platforms, p => p.Region == "ru");
            Assert.Contains(_tree.Platforms, p => p.Region == "ua");
        }

        [Fact]
        public void AddSubTree_ShouldAddSubTreeCorrectly()
        {
            // Arrange
            var platformName = "Yandex.Direct";
            var path = "ru";

            // Act
            _treeService.AddSubTree(path, platformName, _tree);

            // Assert
            Assert.Contains(_tree.Platforms, p => p.Region == "ru");    // Нахождение узла региона 'ru' в дереве
            Assert.Contains(_tree.Platforms[1].PlatformNames, name => name == platformName);    // Проверка корректности добавления платформы
        }

        [Fact]
        public void FindPlatformNames_ShouldReturnCorrectPlatforms()
        {
            // Arrange
            var platformNode2 = new PlatformNode("ru", _tree.Platforms[0]);
            var platformNode3 = new PlatformNode("ua", _tree.Platforms[0]);

            platformNode2.PlatformNames.Add("Yandex.Direct");
            platformNode3.PlatformNames.Add("Google Ads");

            _tree.Add(platformNode2);
            _tree.Add(platformNode3);

            // Act
            var resultRu = _treeService.FindPlatformNames("ru", _tree);
            var resultUa = _treeService.FindPlatformNames("ua", _tree);

            // Assert
            Assert.Contains("Yandex.Direct", resultRu);
            Assert.Contains("Google Ads", resultUa);
        }

        [Fact]
        public void AddSubTree_ShouldWriteToFile_WhenPathIsNotNull()
        {
            // Arrange
            var platformName = "Yandex.Direct";
            var path = "test_path.txt";
            var platformNode = new PlatformNode("Мир");
            _tree.Add(platformNode); // Добавляем корневой узел "Мир"

            // Act
            _treeService.AddSubTree("/ru", platformName, _tree, path);

            // Assert
            Assert.True(File.Exists(path));  // Проверка, что файл был создан

            // Cleanup
            File.Delete(path);
        }
    }
}
