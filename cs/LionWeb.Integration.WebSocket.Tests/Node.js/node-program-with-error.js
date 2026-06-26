// Copyright 2025 LionWeb Project
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
//
// SPDX-FileCopyrightText: 2025 LionWeb Project
// SPDX-License-Identifier: Apache-2.0

setTimeout(() => {
    console.log("started (press Ctrl-C to exit)")
    const scriptName = __filename.substring(__filename.lastIndexOf("/") + 1)
    console.error(`This is an "error" printed to stderr by the ${scriptName} program as part of the RunNodeProgram_With_Error unit test — please ignore!`);
    setInterval(() => {}, 1 << 25)  // leave on "forever" (=9h19m14s)
}, 300)
