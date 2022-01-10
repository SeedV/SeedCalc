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

using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace SeedCalc {
  public static class RequestReviewUtils {
    private const string _userPrefKeyCounter = "process-completed-counter";
    private const string _userPrefKeyLastVersion = "last-version-promoted-for-review";
    private const int _threshold = 50;

    // Increases the counter stored in the user preferences. If the counter exceeds the threshold
    // for the current version, invokes the API call to request an app store review.
    //
    // See https://developer.apple.com/documentation/storekit/requesting_app_store_reviews for a
    // reference code.
    public static void CountAndRequestReviewIfNecessary() {

      // For now, only iOS build has this feature enabled.
      #if UNITY_IOS

      int counter = PlayerPrefs.GetInt(_userPrefKeyCounter, 0);
      PlayerPrefs.SetInt(_userPrefKeyCounter, ++counter);
      if (counter >= _threshold) {
        PlayerPrefs.SetInt(_userPrefKeyCounter, 0);
        string lastVersion = PlayerPrefs.GetString(_userPrefKeyLastVersion, "");
        if (Application.version != lastVersion) {
          Debug.Log("Try to request app store review.");
          Device.RequestStoreReview();
          PlayerPrefs.SetString(_userPrefKeyLastVersion, Application.version);
        }
      }

      #endif

    }
  }
}
