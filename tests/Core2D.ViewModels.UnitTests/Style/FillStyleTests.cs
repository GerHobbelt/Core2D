﻿#nullable disable
using Core2D.Model;
using Xunit;

namespace Core2D.ViewModels.UnitTests.Style
{
    public class FillStyleTests
    {
        private readonly IFactory _factory = new Factory(null);

        [Fact]
        [Trait("Core2D.Style", "Style")]
        public void Inherits_From_ViewModelBase()
        {
            var target = _factory.CreateFillStyle();
            Assert.True(target is ViewModelBase);
        }
    }
}
