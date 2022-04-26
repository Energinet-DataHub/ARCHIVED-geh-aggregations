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
from http.server import HTTPServer, BaseHTTPRequestHandler
from threading import Thread


@pytest.fixture(scope="session")
def snapshot_http_server_url():

    class HTTPHandler(BaseHTTPRequestHandler):
        def do_POST(self):
            content = "Hello, World!"
            self.send_response(200)
            self.send_header("Content-Type", "text/html")
            self.send_header("Content-Length", str(len(content)))
            self.end_headers()
            self.wfile.write(content.encode("ascii"))

    port = 8000
    with HTTPServer(("", port), HTTPHandler) as httpd:
        print("serving at port", port)
        thread = Thread(target=httpd.serve_forever, daemon=True)
        thread.start()
        yield f"http://localhost:{port}"
        httpd.shutdown()
        thread.join()
