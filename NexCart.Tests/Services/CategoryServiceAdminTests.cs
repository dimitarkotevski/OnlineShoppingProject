using System;
using System.Collections.Generic;
using System.Linq;
using NexCart.Business.Exceptions;
using NexCart.Business.Requests;
using NexCart.Business.Services;
using NexCart.Business.ViewModels;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NSubstitute;
using Xunit;

namespace NexCart.Tests.Services
{
    public class CategoryServiceAdminTests
    {
        private readonly ICategoryRepository _categoryRepo = Substitute.For<ICategoryRepository>();
        private readonly List<Category> _categories = new();

        public CategoryServiceAdminTests()
        {
            _categoryRepo.All().Returns(_ => _categories.AsQueryable());
            _categoryRepo.FindBy(Arg.Any<Guid>()).Returns(call => _categories.FirstOrDefault(c => c.Id == call.Arg<Guid>()));
            _categoryRepo.GetProductsByCategory(Arg.Any<Category>())
                .Returns(call => call.Arg<Category>().Products.AsQueryable());
        }

        private CategoryService CreateSut()
        {
            return new CategoryService(
                _categoryRepo,
                Substitute.For<IOrderRepository>(),
                Substitute.For<IFavouriteProductsRepository>(),
                Substitute.For<IShoppingCartDetailRepository>(),
                Substitute.For<IProductSortService>(),
                Substitute.For<IUnitOfWork>());
        }

        private Category AddCategory(string name, bool active = true, int products = 0)
        {
            var category = new Category { Id = Guid.NewGuid(), Name = name, Status = active, Url = "", Products = new List<Product>() };
            for (var i = 0; i < products; i++)
            {
                category.Products.Add(new Product { Id = Guid.NewGuid(), Name = $"{name} {i}", Category = category });
            }
            _categories.Add(category);
            return category;
        }

        [Fact]
        public void Delete_refuses_a_category_that_still_has_products()
        {
            var laptops = AddCategory("Laptops", products: 2);

            var error = Assert.Throws<BllException>(() => CreateSut().Delete(laptops.Id));

            Assert.Contains("2 product", error.Message);
            _categoryRepo.DidNotReceive().Delete(Arg.Any<Category>());
        }

        [Fact]
        public void Delete_removes_an_empty_category()
        {
            var empty = AddCategory("Empty");

            CreateSut().Delete(empty.Id);

            _categoryRepo.Received(1).Delete(empty);
        }

        [Fact]
        public void Create_rejects_a_duplicate_name()
        {
            AddCategory("Laptops");

            Assert.Throws<BllException>(() => CreateSut().Create(new CategoryViewModel { Name = " Laptops " }));
            _categoryRepo.DidNotReceive().Add(Arg.Any<Category>());
        }

        [Fact]
        public void Create_trims_the_name_and_builds_the_slug()
        {
            CreateSut().Create(new CategoryViewModel { Name = "  Headphones & Audio ", Status = false });

            _categoryRepo.Received(1).Add(Arg.Is<Category>(c =>
                c.Name == "Headphones & Audio" && c.Url == "headphones-audio" && !c.Status));
        }

        [Fact]
        public void Update_may_keep_its_own_name()
        {
            var laptops = AddCategory("Laptops");

            CreateSut().Update(new CategoryViewModel { Id = laptops.Id, Name = "Laptops", Status = false });

            Assert.False(laptops.Status);
            _categoryRepo.Received(1).Update(laptops);
        }

        [Fact]
        public void GetTable_filters_by_status_and_counts_products()
        {
            AddCategory("Laptops", products: 3);
            AddCategory("Hidden", active: false);

            var result = CreateSut().GetTable(new RequestModel { IsActive = true, PageNeeded = 1, Size = 10 });

            var row = Assert.Single(result.Items);
            Assert.Equal("Laptops", row.Name);
            Assert.Equal(3, row.ProductCount);
        }

        [Fact]
        public void GetAll_for_the_storefront_hides_inactive_categories()
        {
            AddCategory("Laptops");
            AddCategory("Hidden", active: false);

            var names = CreateSut().GetAll(activeOnly: true).Select(c => c.Name);

            Assert.Equal(new[] { "Laptops" }, names);
        }
    }
}
