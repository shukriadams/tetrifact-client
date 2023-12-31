from http.server import BaseHTTPRequestHandler, HTTPServer
import re as regex
import os

class MyServer(BaseHTTPRequestHandler):

    def do_GET(self):
        if self.path == '/v1/packages':
            self.do_packages()
        elif self.path.startswith('/v1/packages/'):
            packageid = regex.search('\/v1\/packages\/(.*)', self.path).group(1)
            print(self.path)
            print(packageid)
            self.do_package(packageid)
        else:
            self.do_unhandled()

    def do_packages(self):
        self.send_response(200)
        self.send_header('Content-type','text/json')
        self.end_headers()

        with open('./packages.json', 'rb') as file: 
            self.wfile.write(file.read()) # Read the file and send the contents 

    def do_package(self, package):
    
        packagePath = f'./packages/{package}.json'
    
        if not os.path.isfile(packagePath):
            self.send_response(404)
            self.send_header('Content-type','text/html')
            self.end_headers()
            self.wfile.write(f'Package {packagePath} not found'.encode())
            return

        self.send_response(200)
        self.send_header('Content-type','text/json')
        self.end_headers()

        with open(packagePath, 'rb') as file: 
            self.wfile.write(file.read()) # Read the file and send the contents 

    def do_unhandled(self):
        self.send_response(200)
        self.send_header('Content-type','text/html')
        self.end_headers()

        self.wfile.write('Unhandled route - try something else'.encode())


print('server started')
myServer = HTTPServer(('localhost', 8000), MyServer)
myServer.serve_forever()