// // Copyright 2024 TRUMPF Laser GmbH
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// // SPDX-FileCopyrightText: 2024 TRUMPF Laser GmbH
// // SPDX-License-Identifier: Apache-2.0

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable InconsistentNaming

namespace LionWeb.Integration.WebSocket.Client;

public enum Tasks
{
    SignOn,
    SignOff,
    Wait,
    AddStringValue_0_1,
    SetStringValue_0_1,
    DeleteStringValue_0_1,
    AddName_Containment_0_1,
    AddAnnotation,
    AddAnnotations,
    AddAnnotation_to_Containment_0_1,
    DeleteAnnotation,
    MoveAnnotationInSameParent,
    MoveAnnotationFromOtherParent,
    AddReference_0_1_to_Containment_0_1,
    AddReference_0_1_to_Containment_1,
    DeleteReference_0_1,
    AddContainment_0_1,
    AddContainment_1,
    ReplaceContainment_0_1,
    DeleteContainment_0_1,
    AddContainment_0_1_Containment_0_1,
    AddContainment_1_Containment_0_1,
    AddContainment_0_n,
    AddContainment_0_n_Containment_0_n,
    AddContainment_1_n,
    MoveAndReplaceChildFromOtherContainment_Single,
    MoveAndReplaceChildFromOtherContainmentInSameParent_Single,
    MoveAndReplaceChildFromOtherContainment_Multiple,
    MoveChildInSameContainment,
    MoveChildFromOtherContainment_Single,
    MoveChildFromOtherContainment_Multiple,
    MoveChildFromOtherContainmentInSameParent_Single,
    AddPartition,
    MoveChildFromOtherContainmentInSameParent_Multiple
}