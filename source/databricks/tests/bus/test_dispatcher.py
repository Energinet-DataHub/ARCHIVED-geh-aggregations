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

from unittest.mock import Mock
from dataclasses import dataclass

from geh_stream.bus.dispatcher import MessageDispatcher
from geh_stream.bus.broker import Message


@dataclass
class Message1(Message):
    something: str


@dataclass
class Message2(Message):
    something: str


class TestMessageDispatcher:

    def test__handler_exists_for_type__should_invoke_handler(self):

        # Arrange
        handler1 = Mock()
        handler2 = Mock()

        sut = MessageDispatcher({
            Message1: handler1,
            Message2: handler2
        })

        msg1 = Message1(something='something')

        # Act
        sut(msg1)

        # Assert
        handler1.assert_called_once_with(msg1)
        handler2.assert_not_called()

    def test__handler_does_not_exist_for_type__should_not_invoke_handler(self):

        # Arrange
        handler1 = Mock()

        sut = MessageDispatcher({
            Message1: handler1
        })

        msg2 = Message2(something='something')

        # Act
        sut(msg2)

        # Assert
        handler1.assert_not_called()

    def test__set_master_data_root_path__master_data_root_path_set_on_dispatcher_object(self):

        # Arrange
        sut = MessageDispatcher()

        expected_path = "root_path"

        # Act
        sut.set_master_data_root_path(expected_path)

        # Assert
        assert sut.master_data_root_path == expected_path
