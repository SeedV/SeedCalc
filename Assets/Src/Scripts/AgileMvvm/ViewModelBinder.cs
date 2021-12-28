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
using System.Collections.Generic;
using System.Reflection;

namespace AgileMvvm {
  // The class to manage binding relationships.
  internal class ViewModelBinder {
    // The mapping from bound properties to their UpdatedEvent instances. Every bound property has a
    // separate UpdatedEvent instance.
    protected readonly Dictionary<PropertyInfo, UpdatedEvent> _updatedEvents =
        new Dictionary<PropertyInfo, UpdatedEvent>();

    internal void Clear() {
      _updatedEvents.Clear();
    }

    internal void BindOneWay(
        PropertyInfo viewModelProperty,
        object view,
        PropertyInfo viewProperty) {
      CreateUpdatedEventIfNotExist(viewModelProperty);
      _updatedEvents[viewModelProperty].AddHandler((object sender, UpdatedEvent.Args args) => {
        viewProperty.GetSetMethod().Invoke(view, new object[] { args.Value });
      });
    }

    internal void BindOneWay(
        PropertyInfo viewModelProperty,
        EventHandler<UpdatedEvent.Args> updatedHandler) {
      CreateUpdatedEventIfNotExist(viewModelProperty);
      _updatedEvents[viewModelProperty].AddHandler(updatedHandler);
    }

    internal void RaiseUpdatedEvent(PropertyInfo viewModelProperty, object value, object sender) {
      if (_updatedEvents.ContainsKey(viewModelProperty)) {
        _updatedEvents[viewModelProperty].Raise(sender, new UpdatedEvent.Args { Value = value });
      }
    }

    private void CreateUpdatedEventIfNotExist(PropertyInfo viewModelProperty) {
      if (!_updatedEvents.ContainsKey(viewModelProperty)) {
        _updatedEvents.Add(viewModelProperty, new UpdatedEvent());
      }
    }
  }
}
