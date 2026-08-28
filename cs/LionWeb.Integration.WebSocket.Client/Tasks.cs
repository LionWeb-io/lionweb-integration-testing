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

// ReSharper disable InconsistentNaming

namespace LionWeb.Integration.WebSocket.Client;

public enum Tasks
{
    /// Signs this client on to repository "repoId"
    SignOn,

    /// Subscribes this client to changing partitions
    SubscribeToChangingPartitions,

    /// Signs this client off
    SignOff,

    /// Waits for one received event
    Wait,

    /// Adds partition
    /// TestPartition(id: "partition") {
    ///   Data = DataTypeTestConcept(id: "data"),
    ///   Links = [ LinkTestConcept(id: "link") ]
    /// }
    AddPartition,

    #region Properties

    /// Adds
    /// "new property" value
    /// to
    /// partition.Data.StringValue_0_1
    AddStringValue_0_1,

    /// Sets
    /// "changed property" value
    /// to
    /// partition.Data.StringValue_0_1
    SetStringValue_0_1,

    /// Removes
    /// value
    /// of
    /// partition.Data.StringValue_0_1
    DeleteStringValue_0_1,

    /// Sets
    /// "my name"
    /// to
    /// partition.Links[0].Containment_0_1.Name
    AddName_Containment_0_1,

    #endregion

    #region Annotations

    /// Adds
    /// TestAnnotation(id: "annotation")
    /// to
    /// partition
    AddAnnotation,

    /// Adds
    /// TestAnnotation(id: "annotation0")
    /// and
    /// TestAnnotation(id: "annotation1")
    /// to
    /// partition
    AddAnnotations,

    /// Adds
    /// TestAnnotation(id: "annotation")
    /// to
    /// partition.Links[0].Containment_0_1
    AddAnnotation_to_Containment_0_1,

    /// Adds
    /// TestAnnotation(id: "annotation") {
    ///   Ref = (M2)DataTypeTestConcept.booleanValue_0_1
    /// }
    /// to
    /// partition
    AddAnnotationWithLanguageReference,

    /// Deletes
    /// all annotations
    /// from
    /// partition
    DeleteAnnotation,

    /// Moves
    /// the last annotation of partition
    /// to
    /// index 0 of partition
    MoveAnnotationInSameParent,

    /// Moves
    /// all annotations of partition.Links[0].Containment_0_1
    /// to
    /// partition
    MoveAnnotationFromOtherParent,

    /// Replaces
    /// the first annotation of partition
    /// with
    /// the last annotation of partition
    MoveAndReplaceAnnotationInSameParent,

    /// Replaces
    /// the first annotation of partition
    /// with
    /// the first annotation of partition.Links[0].Containment_0_1 
    MoveAndReplaceAnnotationFromOtherParent,

    #endregion

    #region References

    /// Makes
    /// partition.Links[0].Reference_0_1
    /// refer to
    /// partition.Links[0].Containment_0_1
    AddReference_0_1_to_Containment_0_1,

    /// Makes
    /// partition.Links[0].Reference_0_1
    /// refer to
    /// partition.Links[0].Containment_1
    AddReference_0_1_to_Containment_1,

    /// Deletes
    /// target
    /// of
    /// partition.Links[0].Reference_0_1
    DeleteReference_0_1,

    #endregion

    #region Containments

    /// Adds
    /// LinkTestConcept(id: "containment_0_1")
    /// to
    /// partition.Links[0].Containment_0_1
    AddContainment_0_1,

    /// Adds
    /// LinkTestConcept(id: "containment_1")
    /// to
    /// partition.Links[0].Containment_1
    AddContainment_1,

    /// Replaces
    /// partition.Links[0].Containment_0_1
    /// with
    /// LinkTestConcept(id: "substitute")
    ReplaceContainment_0_1,

    /// Deletes
    /// partition.Links[0].Containment_0_1
    DeleteContainment_0_1,

    /// Adds
    /// LinkTestConcept(id: "containment_0_1_containment_0_1")
    /// to
    /// partition.Links[0].Containment_0_1.Containment_0_1
    AddContainment_0_1_Containment_0_1,

    /// Adds
    /// LinkTestConcept(id: "containment_1_containment_0_1")
    /// to
    /// partition.Links[0].Containment_1.Containment_0_1
    AddContainment_1_Containment_0_1,

    /// Adds
    /// LinkTestConcept(id: "containment_0_n_child0")
    /// and
    /// LinkTestConcept(id: "containment_0_n_child1")
    /// to
    /// partition.Links[0].Containment_0_n
    AddContainment_0_n,

    /// Adds
    /// LinkTestConcept(id: "containment_0_n_child0_deep") {
    ///   Containment_0_n = [LinkTestConcept(id: "containment_0_n_containment_0_n_child0")]
    /// }
    /// to
    /// partition.Links[0].Containment_0_n
    AddContainment_0_n_Containment_0_n,

    /// Adds
    /// LinkTestConcept(id: "containment_1_n_child0")
    /// and
    /// LinkTestConcept(id: "containment_1_n_child1")
    /// to
    /// partition.Links[0].Containment_1_n
    AddContainment_1_n,

    #region Move

    /// Moves
    /// the first entry of partition.Links[0].Containment_0_n
    /// to
    /// the last entry of partition.Links[0].Containment_0_n
    MoveChildInSameContainment_Forward,

    /// Moves
    /// the last entry of partition.Links[0].Containment_0_n
    /// to
    /// the first entry of partition.Links[0].Containment_0_n
    MoveChildInSameContainment_Backward,

    /// Sets
    /// partition.Links[0].Containment_1
    /// to
    /// partition.Links[0].Containment_0_1
    MoveChildFromOtherContainmentInSameParent_Single,

    /// Moves
    /// the last entry of partition.Links[0].Containment_0_n
    /// to
    /// the second entry of partition.Links[0].Containment_1_n
    MoveChildFromOtherContainmentInSameParent_Multiple,

    /// Sets
    /// partition.Links[0].Containment_1
    /// to
    /// partition.Links[0].Containment_0_1!.Containment_0_1
    MoveChildFromOtherContainment_Single,

    /// Moves
    /// the first entry of Containment_0_n of the last entry of partition.Links[0].Containment_0_n
    /// to
    /// the second entry of partition.Links[0].Containment_1_n
    MoveChildFromOtherContainment_Multiple,

    #endregion

    #region Move and Replace

    /// Replaces
    /// the last entry of partition.Links[0].Containment_0_n
    /// with
    /// the first entry of partition.Links[0].Containment_0_n
    MoveAndReplaceChildInSameContainment_Forward,

    /// Replaces
    /// the first entry of partition.Links[0].Containment_0_n
    /// with
    /// the last entry of partition.Links[0].Containment_0_n
    MoveAndReplaceChildInSameContainment_Backward,

    /// Replaces
    /// partition.Links[0].Containment_1
    /// with
    /// partition.Links[0].Containment_0_1
    MoveAndReplaceChildFromOtherContainmentInSameParent_Single,

    /// Replaces
    /// the second entry of partition.Links[0].Containment_1_n
    /// with
    /// the last entry of partition.Links[0].Containment_0_n
    MoveAndReplaceChildFromOtherContainmentInSameParent_Multiple,

    /// Replaces
    /// partition.Links[0].Containment_1.Containment_0_1
    /// with
    /// partition.Links[0].Containment_0_1.Containment_0_1
    MoveAndReplaceChildFromOtherContainment_Single,

    /// Replaces
    /// the last entry of partition.Links[0].Containment_1_n
    /// with
    /// the last entry of Containment_0_n of the last entry of partition.Links[0].Containment_0_n 
    MoveAndReplaceChildFromOtherContainment_Multiple

    #endregion

    #endregion
}