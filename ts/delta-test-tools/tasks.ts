// Copyright 2025 TRUMPF Laser SE and other contributors
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
// SPDX-FileCopyrightText: 2025 TRUMPF Laser SE and other contributors
// SPDX-License-Identifier: Apache-2.0

import type { LionWebClient } from "@lionweb/delta-protocol-client"
import type { ISemanticLogItem, Procedure } from "@lionweb/delta-protocol-common"
import { ansi, ClientReceivedMessage } from "@lionweb/delta-protocol-common"
import type { LionWebId } from "@lionweb/json"
import { lastOfArray } from "@lionweb/ts-utils"
import {
    DataTypeTestConcept,
    LinkTestConcept,
    TestAnnotation,
    TestLanguageBase,
    TestPartition
} from "@lionweb/class-core-test-language"

import { waitUntil } from "./async.js"

const { clientInfo, genericWarning } = ansi


/**
 * **DEV note**: run
 *
 *  $ ./ts/scripts/extract-tasks-from-csharp.ts
 *
  * from this repo’s root to generate the contents of the following object.
 */
export const recognizedTasks: Record<string, boolean> = {
    "SignOn": true,
    "SubscribeToChangingPartitions": true,
    "SignOff": true,
    "Wait": true,
    "AddPartition": true,
    "AddStringValue_0_1": true,
    "SetStringValue_0_1": true,
    "DeleteStringValue_0_1": true,
    "AddName_Containment_0_1": true,
    "AddAnnotation": true,
    "AddAnnotations": true,
    "AddAnnotation_to_Containment_0_1": true,
    "AddAnnotationWithLanguageReference": true,
    "DeleteAnnotation": true,
    "MoveAnnotationInSameParent": true,
    "MoveAnnotationFromOtherParent": true,
    "MoveAndReplaceAnnotationInSameParent": true,
    "MoveAndReplaceAnnotationFromOtherParent": true,
    "AddReference_0_1_to_Containment_0_1": true,
    "AddReference_0_1_to_Containment_1": true,
    "DeleteReference_0_1": true,
    "AddContainment_0_1": true,
    "AddContainment_1": true,
    "ReplaceContainment_0_1": true,
    "DeleteContainment_0_1": true,
    "AddContainment_0_1_Containment_0_1": true,
    "AddContainment_1_Containment_0_1": true,
    "AddContainment_0_n": true,
    "AddContainment_0_n_Containment_0_n": true,
    "AddContainment_1_n": true,
    "MoveChildInSameContainment_Forward": true,
    "MoveChildInSameContainment_Backward": true,
    "MoveChildFromOtherContainmentInSameParent_Single": true,
    "MoveChildFromOtherContainmentInSameParent_Multiple": true,
    "MoveChildFromOtherContainment_Single": true,
    "MoveChildFromOtherContainment_Multiple": true,
    "MoveAndReplaceChildInSameContainment_Forward": true,
    "MoveAndReplaceChildInSameContainment_Backward": true,
    "MoveAndReplaceChildFromOtherContainmentInSameParent_Single": true,
    "MoveAndReplaceChildFromOtherContainmentInSameParent_Multiple": true,
    "MoveAndReplaceChildFromOtherContainment_Single": true,
    "MoveAndReplaceChildFromOtherContainment_Multiple": true,
    "TryToWriteProtocolLog": true
}


const testLanguageBase = TestLanguageBase.INSTANCE


export const taskExecutor = (lionWebClient: LionWebClient, semanticLogItems: ISemanticLogItem[], tryToWriteProtocolLog: Procedure<void>) => {

    const numberOfReceivedMessages = () =>
        semanticLogItems.filter((item) => item instanceof ClientReceivedMessage).length

    const waitForReceivedMessages = async (numberOfMessagesToReceive: number) => {
        const expectedNumber = numberOfReceivedMessages() + numberOfMessagesToReceive  // (precompute here)
        return waitUntil(10, () => numberOfReceivedMessages() >= expectedNumber)
            .then(() => {
                console.log(clientInfo(`(client "${lionWebClient.clientId}" received a total of ${numberOfReceivedMessages()} messages so far)`))
            })
    }

    const annotation = (id: LionWebId) =>
        lionWebClient.forest.createNode(testLanguageBase.TestAnnotation, id) as TestAnnotation

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const as = <T>(obj: object, classConstructor: new (...args: any[]) => T, customErrorMessage?: string) => {
        if (obj instanceof classConstructor) {
            return obj as T
        }
        throw new Error(customErrorMessage ?? `object is not of class ${classConstructor.name} but of class ${obj.constructor.name}`)
    }

    const thePartition = () => {
        const nPartitions = lionWebClient.forest.partitions.length
        if (nPartitions !== 1) {
            throw new Error(`${nPartitions > 1 ? `${nPartitions} partitions` : `no partition`} added to the forest of LionWeb client with ID="${lionWebClient.clientId}" — expected exactly 1`)
        }
        return as(lionWebClient.forest.partitions[0], TestPartition, `partition at index 0 in forest is not a ${testLanguageBase.TestPartition.name} but a ${lionWebClient.forest.partitions[0].constructor.name}`)
    }

    const linkTestConcept = (id?: LionWebId) => {
        if (id === undefined) {
            const nLinks = thePartition().links.length
            if (nLinks !== 1) {
                throw new Error(`${nLinks > 1 ? `${nLinks} links` : `no links`} added to the test partition (=root of forest) on LionWeb client with ID="${lionWebClient.clientId}" — expected exactly 1`)
            }
            return thePartition().links[0]  // ~ partition.Links[0] in C#
        }
        return lionWebClient.forest.createNode(testLanguageBase.LinkTestConcept, id) as LinkTestConcept
    }

    const dataTypeTestConcept = () =>
        thePartition().data!   // ~ partition.Data in C#

    return async (task: keyof typeof recognizedTasks, queryId: string) => {
        console.log(clientInfo(`client "${lionWebClient.clientId}" is executing task "${task}"`))
        switch (task) {

            // queries:

            case "SignOn":
                return await lionWebClient.signOn(queryId, "myRepo")

            case "SubscribeToChangingPartitions":
                return await lionWebClient.subscribeToChangingPartitions(queryId, {
                    creation: true,
                    deletion: true
                })

            case "SignOff":
                return await lionWebClient.signOff(queryId)


            // {deltas} partitions:

            case "AddPartition": {
                const partition = lionWebClient.forest.createNode(testLanguageBase.TestPartition, "partition") as TestPartition
                partition.data = lionWebClient.forest.createNode(testLanguageBase.DataTypeTestConcept, "data") as DataTypeTestConcept
                partition.addLinks(lionWebClient.forest.createNode(testLanguageBase.LinkTestConcept, "link") as LinkTestConcept)
                lionWebClient.addPartition(partition)
                return waitForReceivedMessages(1)
            }


            // {deltas} properties:

            case "AddStringValue_0_1":
                dataTypeTestConcept().stringValue_0_1 = "new property"
                return waitForReceivedMessages(1)

            case "SetStringValue_0_1":
                dataTypeTestConcept().stringValue_0_1 = "changed property"
                return waitForReceivedMessages(1)

            case "DeleteStringValue_0_1":
                dataTypeTestConcept().stringValue_0_1 = undefined
                return waitForReceivedMessages(1)

            case "AddName_Containment_0_1":
                linkTestConcept().containment_0_1!.name = "my name"
                return waitForReceivedMessages(1)


            // {deltas} annotations:

            case "AddAnnotation":
                thePartition().addAnnotation(annotation("annotation"))
                return waitForReceivedMessages(1)

            case "AddAnnotations":
                thePartition().addAnnotation(annotation("annotation0"));   // (keep ;!)
                thePartition().addAnnotation(annotation("annotation1"))
                return waitForReceivedMessages(2)

            case "AddAnnotation_to_Containment_0_1":
                linkTestConcept().containment_0_1!.addAnnotation(annotation("annotation"))
                return waitForReceivedMessages(1)

            case "AddAnnotationWithLanguageReference": {
                const ann = annotation("annotation")
                ann.ref = testLanguageBase.DataTypeTestConcept_booleanValue_0_1
                thePartition().addAnnotation(ann)
                return waitForReceivedMessages(1)
            }

            case "DeleteAnnotation": {
                const nAnnotations = thePartition().annotations.length
                thePartition().annotations.forEach((annotation) => {
                    thePartition().removeAnnotation(annotation)
                })
                return waitForReceivedMessages(nAnnotations)
            }

            case "MoveAnnotationInSameParent":
                thePartition().insertAnnotationAtIndex(lastOfArray(thePartition().annotations), 0)
                return waitForReceivedMessages(1)

            case "MoveAnnotationFromOtherParent":
                thePartition().addAnnotation(linkTestConcept().containment_0_1!.annotations[0])
                return waitForReceivedMessages(1)

            case "MoveAndReplaceAnnotationInSameParent": {
                const oldIndex = thePartition().annotations.length - 1
                thePartition().moveAndReplaceAnnotationOffsetBased(oldIndex, -oldIndex)
                return waitForReceivedMessages(1)
            }

            case "MoveAndReplaceAnnotationFromOtherParent": {
                thePartition().replaceAnnotationAtIndex(linkTestConcept().containment_0_1!.annotations[0], 0)
                return waitForReceivedMessages(1)
            }


            // {deltas} references:

            case "AddReference_0_1_to_Containment_0_1":
                linkTestConcept().reference_0_1 = linkTestConcept().containment_0_1
                return waitForReceivedMessages(1)

            case "AddReference_0_1_to_Containment_1":
                linkTestConcept().reference_0_1 = linkTestConcept().containment_1
                return waitForReceivedMessages(1)

            case "DeleteReference_0_1":
                linkTestConcept().reference_0_1 = undefined
                return waitForReceivedMessages(1)


            // {deltas} containments:

            case "AddContainment_0_1":
                linkTestConcept().containment_0_1 = linkTestConcept("containment_0_1")
                return waitForReceivedMessages(1)

            case "AddContainment_1":
                linkTestConcept().containment_1 = linkTestConcept("containment_1")
                return waitForReceivedMessages(1)

            case "ReplaceContainment_0_1":
                linkTestConcept().replaceContainment_0_1With(linkTestConcept("substitute"))
                return waitForReceivedMessages(1)

            case "DeleteContainment_0_1":
                linkTestConcept().containment_0_1 = undefined
                return waitForReceivedMessages(1)

            case "AddContainment_0_1_Containment_0_1":
                linkTestConcept().containment_0_1!.containment_0_1 = linkTestConcept("containment_0_1_containment_0_1")
                return waitForReceivedMessages(1)

            case "AddContainment_1_Containment_0_1":
                linkTestConcept().containment_1.containment_0_1 = linkTestConcept("containment_1_containment_0_1")
                return waitForReceivedMessages(1)

            case "AddContainment_0_n":
                linkTestConcept().addContainment_0_n(linkTestConcept("containment_0_n_child0"));   // (keep ;!)
                linkTestConcept().addContainment_0_n(linkTestConcept("containment_0_n_child1"))
                return waitForReceivedMessages(2)

            case "AddContainment_0_n_Containment_0_n": {
                const outerChild = linkTestConcept("containment_0_n_child0_deep")
                outerChild.addContainment_0_n(linkTestConcept("containment_0_n_containment_0_n_child0"))
                linkTestConcept().addContainment_0_n(outerChild)
                return waitForReceivedMessages(1)
            }

            case "AddContainment_1_n":
                linkTestConcept().addContainment_1_n(linkTestConcept("containment_1_n_child0"));   // (keep ;!)
                linkTestConcept().addContainment_1_n(linkTestConcept("containment_1_n_child1"))
                return waitForReceivedMessages(2)

            // {deltas} (containments.)move:

            case "MoveChildInSameContainment_Forward": {
                const indexLastChild = linkTestConcept().containment_0_n.length - 1
                linkTestConcept().moveContainment_0_nOffsetBased(0, indexLastChild)   // -> index ^1
                return waitForReceivedMessages(1)
            }

            case "MoveChildInSameContainment_Backward": {
                const indexLastChild = linkTestConcept().containment_0_n.length - 1
                linkTestConcept().moveContainment_0_nOffsetBased(indexLastChild, -indexLastChild)   // -> index 0
                return waitForReceivedMessages(1)
            }

            case "MoveChildFromOtherContainmentInSameParent_Single":
                linkTestConcept().containment_1 = linkTestConcept().containment_0_1!
                return waitForReceivedMessages(1)

            case "MoveChildFromOtherContainmentInSameParent_Multiple":
                linkTestConcept().addContainment_1_nAtIndex(lastOfArray(linkTestConcept().containment_0_n), 1)
                return waitForReceivedMessages(1)

            case "MoveChildFromOtherContainment_Single":
                linkTestConcept().containment_1 = linkTestConcept().containment_0_1!.containment_0_1!
                return waitForReceivedMessages(1)

            case "MoveChildFromOtherContainment_Multiple":
                linkTestConcept().addContainment_1_nAtIndex(lastOfArray(linkTestConcept().containment_0_n).containment_0_n[0], 1)
                return waitForReceivedMessages(1)

            // {deltas} (containments.)move+replace:

            case "MoveAndReplaceChildInSameContainment_Forward": {
                const indexLastChild = linkTestConcept().containment_0_n.length - 1
                linkTestConcept().moveAndReplaceContainment_0_nOffsetBased(0, indexLastChild)
                return waitForReceivedMessages(1)
            }

            case "MoveAndReplaceChildInSameContainment_Backward": {
                const oldIndex = linkTestConcept().containment_0_n.length - 1
                linkTestConcept().moveAndReplaceContainment_0_nOffsetBased(oldIndex, -oldIndex)
                return waitForReceivedMessages(1)
            }

            case "MoveAndReplaceChildFromOtherContainmentInSameParent_Single":
                linkTestConcept().replaceContainment_1With(linkTestConcept().containment_0_1!)
                return waitForReceivedMessages(1)

            case "MoveAndReplaceChildFromOtherContainmentInSameParent_Multiple":
                linkTestConcept().replaceContainment_1_nAtIndex(lastOfArray(linkTestConcept().containment_0_n), 1)
                return waitForReceivedMessages(1)

            case "MoveAndReplaceChildFromOtherContainment_Single":
                linkTestConcept().containment_1.replaceContainment_0_1With(linkTestConcept().containment_0_1!.containment_0_1!)
                return waitForReceivedMessages(1)

            case "MoveAndReplaceChildFromOtherContainment_Multiple":
                if (linkTestConcept().containment_1_n.length === 0) {
                    throw new Error(`can't replace an item of an array with no items`)
                }
                linkTestConcept().replaceContainment_1_nAtIndex(
                    lastOfArray(lastOfArray(linkTestConcept().containment_0_n).containment_0_n),
                    linkTestConcept().containment_1_n.length - 1
                )
                return waitForReceivedMessages(1)


            // (auxiliary/special:)

            case "Wait":
                return waitForReceivedMessages(1)

            case "TryToWriteProtocolLog":
                return tryToWriteProtocolLog()


            default: {
                console.log(genericWarning(`task "${task}" is known but not implemented => ignored`))
                return Promise.resolve()
            }

        }
    }
}

