// Copyright 2021-2022 The SeedV Lab.
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
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AgileMvvm {
  // The ViewModel class that maintains models and raises model update events to the bound Views. It
  // also provides the utility methods to manage the binding relationships.
  //
  // In AgileMvvm, the relationships among View, ViewModel and Model are:
  //
  // * The Model does not know about the View or the ViewModel.
  // * The View does not know about the Model.
  // * The View and the ViewModel communicate with each other with bindings.
  // * Different Views or multiple View instances can be bound to the same ViewModel.
  // * The ViewModel does not know about the View, but knows and maintains the Model.
  //
  // Typical MVVM patterns support the following binding modes between the View and the ViewModel:
  //
  // * One-way binding: ViewModel's property to View's property or View's event handler.
  // * One-way-to-source binding: View's property to ViewModel's property. (Rarely used)
  // * Two-way binding: Two-way update between View's property and ViewModel's property.
  // * Event binding: View's event to ViewModel's event handler.
  //
  // As an lightweight framework, AgileMvvm explicitly supports the one-way binding mode via the
  // ViewModel class. To establish a one-way binding between the View and the ViewModel:
  //
  // * Defines your own ViewModel class and implement bindable properties like below:
  //
  //    class PersonViewModel : ViewModel {
  //      private System.Data.DataTable _table;  // Example Model instance owned by ViewModel.
  //      private string _name;
  //      [BindableProperty]
  //      public string Name {
  //        get => _name;
  //        set => MvvmSetter(ref _name, value);
  //      }
  //    }
  //
  // * Defines your View class:
  //
  //    class Dialog {
  //      public string TextField { get; set; }
  //    }
  //
  // * Binds the bindable property of the ViewModel to the target property of the View class:
  //
  //    var dialog = new Dialog();
  //    var personViewModel = new PersonViewModel();
  //    personViewModel.Bind(nameof(personViewModel.Name), dialog, nameof(dialog.TextField));
  //
  // That's it! Any subsequent changes of the ViewModel's bound property will update the View's
  // property.
  //
  // * Or, you can define your own event handler if you want to customize the binding behavior:
  //
  //    var dialog = new Dialog();
  //    var personViewModel = new PersonViewModel();
  //    person.Bind(nameof(personViewModel.Name), (object sender, UpdatedEvent.Args e) => {
  //      dialog.TextField = e.Value.ToString();
  //    });
  //
  // As for other binding modes, AgileMvvm doesn't provide direct support so far. But it is easy to
  // implement them with C#'s event handling mechanism on top of AgileMvvm.
  //
  // TODO: Consider to support other binding modes directly.
  public class ViewModel {
    // The binder to manage the binding relationships.
    private readonly ViewModelBinder _binder = new ViewModelBinder();

    // One-way binding - Binds ViewModel's property to View's property.
    //
    // The type of the View's property must be assignable from the type of the ViewModel's property.
    // See the .Net doc of Type.IsAssignableFrom() for details.
    //
    // TODO: No implicit conversion or explicit ToString() conversion is supported here. Support
    // them later. Or, we can also let the client code to pass in a customized type conversion
    // delegate.
    public void Bind(string viewModelPropertyName, object view, string viewPropertyName) {
      var viewModelProperty = GetPropertyInfo(this, viewModelPropertyName);
      if (!Attribute.IsDefined(viewModelProperty, typeof(BindablePropertyAttribute))) {
        throw new ArgumentException(
            $"ViewModel's property \"{viewModelPropertyName}\" has no [BindableProperty] " +
            "attribute.");
      }
      var sourceType = viewModelProperty.PropertyType;
      var viewProperty = GetPropertyInfo(view, viewPropertyName);
      var targetType = viewProperty.PropertyType;
      if (!targetType.IsAssignableFrom(sourceType)) {
        throw new ArgumentException(
            $"ViewModel's property \"{viewModelPropertyName}\" cannot be assigned to View's " +
            $"property \"{viewPropertyName}\"");
      }
      _binder.BindOneWay(viewModelProperty, view, viewProperty);
    }

    // One-way binding - Binds ViewModel's property to View's event handler.
    public void Bind(string viewModelPropertyName, EventHandler<UpdatedEvent.Args> updatedHandler) {
      var property = GetPropertyInfo(this, viewModelPropertyName);
      _binder.BindOneWay(property, updatedHandler);
    }

    // Clears all the existing bindings.
    public void Clear() {
      _binder.Clear();
    }

    // Utility method for concrete ViewModel classes to implement bound properties' setters.
    protected void MvvmSetter<TProperty>(
        ref TProperty field, TProperty value, [CallerMemberName] string propertyName = "") {
      PropertyInfo property = GetPropertyInfo(this, propertyName);
      field = value;
      RaisePropertyUpdated(property, value);
    }

    private PropertyInfo GetPropertyInfo(object instance, string propertyName) {
      PropertyInfo property = instance.GetType().GetProperty(propertyName);
      if (property is null) {
        throw new ArgumentException($"Property \"{propertyName}\" not found.");
      }
      return property;
    }

    private void RaisePropertyUpdated(PropertyInfo property, object value) {
      _binder.RaiseUpdatedEvent(property, value, this);
    }
  }
}
