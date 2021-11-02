# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import pytest
from dataclasses import dataclass

from geh_stream.bus.broker import Message
from geh_stream.bus.registry import MessageRegistry


@dataclass
class Message1(Message):
    something: str


@dataclass
class Message2(Message):
    something: str


class TestMessageRegistry:

    def test__from_message_types__items_added_to_registry(self):

        # Act
        sut = MessageRegistry.from_message_types(Message1, Message2)

        # Assert
        assert len(sut) == 2
        assert sut['Message1'] == Message1
        assert sut['Message2'] == Message2

    def test__add__items_added_to_registry(self):

        # Arrange
        sut = MessageRegistry()

        # Act
        sut.add(Message1, Message2)

        # Assert
        assert len(sut) == 2
        assert sut['Message1'] == Message1
        assert sut['Message2'] == Message2

    @pytest.mark.parametrize('item', [Message1, Message1(something='something'), 'Message1'])
    def test__contains__item_exists__returns_true(self, item):

        # Arrange
        sut = MessageRegistry()

        # Act
        sut.add(Message1)

        # Assert
        assert item in sut

    @pytest.mark.parametrize('item', [Message2, Message2(something='something'), 'Message2'])
    def test__contains__item_does_not_exist__returns_false(self, item):

        # Arrange
        sut = MessageRegistry()

        # Act
        sut.add(Message1)

        # Assert
        assert item not in sut
