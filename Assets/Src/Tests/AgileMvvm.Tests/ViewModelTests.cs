// Copyright 2021 The Aha001 Team.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using NUnit.Framework;

namespace AgileMvvm.Tests {
  class Animal {
    public string Name { get; set; }
  }

  class Cat : Animal {
  }

  class MyViewModel : ViewModel {
    private string _s;
    private int _i;
    private float _f;
    private Cat _cat = new Cat();

    [BindableProperty]
    public string StrProp {
      get => _s;
      set => MvvmSetter(ref _s, value);
    }

    [BindableProperty]
    public int IntProp {
      get => _i;
      set => MvvmSetter(ref _i, value);
    }

    [BindableProperty]
    public float FloatProp {
      get => _f;
      set => MvvmSetter(ref _f, value);
    }

    [BindableProperty]
    public Cat CatProp {
      get => _cat;
      set => MvvmSetter(ref _cat, value);
    }

    public string NotBindableProp { get; set; }
  }

  class MyView1 {
    public string StrProp { get; set; }
    public int IntProp { get; set; }
    public float FloatProp { get; set; }
    public Animal AnimalProp { get; set; }
    public ViewModel ViewModel { get; set; }
  }

  class MyView2 {
    public string StrProp1 { get; set; }
    public string StrProp2 { get; set; }
    public ViewModel ViewModel { get; set; }
  }

  public class ViewModelTests {
    [Test]
    public void TestInvalidOneWayBindings() {
      var viewModel = new MyViewModel();
      var view = new MyView1();
      Assert.Throws<ArgumentException>(() =>
          viewModel.Bind(
              "InvalidName",
              (object sender, UpdatedEvent.Args e) => view.StrProp = e.Value.ToString()));
      Assert.Throws<ArgumentException>(() => viewModel.Bind("StrProp", view, "IntProp"));
      Assert.Throws<ArgumentException>(() => viewModel.Bind("IntProp", view, "StrProp"));
      Assert.Throws<ArgumentException>(() => viewModel.Bind("IntProp", view, "FloatProp"));
      Assert.Throws<ArgumentException>(() => viewModel.Bind("FloatProp", view, "IntProp"));
      Assert.Throws<ArgumentException>(() => viewModel.Bind("NotBindableProp", view, "StrProp"));
    }

    [Test]
    public void TestSameTypeOneWayBindings() {
      var viewModel = new MyViewModel();
      var view = new MyView1();
      viewModel.Bind("StrProp", view, "StrProp");
      viewModel.StrProp = "Hello";
      Assert.AreEqual("Hello", view.StrProp);

      viewModel.Bind("FloatProp", view, "FloatProp");
      viewModel.FloatProp = 3.14f;
      Assert.AreEqual(3.14f, view.FloatProp);

      viewModel.Clear();
      viewModel.StrProp = "New";
      Assert.AreNotEqual("New", view.StrProp);
    }

    [Test]
    public void TestAssignableTypeOneWayBindings() {
      var viewModel = new MyViewModel();
      var view = new MyView1();
      viewModel.Bind("CatProp", view, "AnimalProp");
      viewModel.CatProp = new Cat { Name = "Tom" };
      Assert.AreEqual("Tom", view.AnimalProp.Name);
    }

    [Test]
    public void TestMultiOneWayBindings() {
      var viewModel = new MyViewModel();
      var view1 = new MyView1();
      var view2 = new MyView2();

      viewModel.Bind("StrProp", view1, "StrProp");
      viewModel.Bind("StrProp", view2, "StrProp1");
      viewModel.Bind("StrProp", view2, "StrProp2");

      viewModel.StrProp = "Hello";
      Assert.AreEqual("Hello", view1.StrProp);
      Assert.AreEqual("Hello", view2.StrProp1);
      Assert.AreEqual("Hello", view2.StrProp2);
    }
  }
}
