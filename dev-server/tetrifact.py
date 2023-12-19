from http.server import BaseHTTPRequestHandler, HTTPServer
import re as regex
import os
import json

port=8000

class TetrifactServer(BaseHTTPRequestHandler):

    def do_GET(self):
        print(f'handling path : {self.path}')
        if self.path == '/v1/packages':
            self.do_packages()
        elif self.path.startswith('/v1/packages/'):
            packageid = regex.search(r'\/v1\/packages\/(.*)', self.path).group(1)
            self.do_package(packageid)
        elif self.path.startswith('/v1/files/'):
            fileid = regex.search(r'\/v1\/files\/(.*)', self.path).group(1)
            self.do_file(fileid)
        elif self.path.startswith('/v1/archives/'):
            archiveid = regex.search(r'\/v1\/archives\/(.*)', self.path).group(1)
            self.do_archive(archiveid)
        elif self.path == '/' or self.path == '':
            self.do_root()
        else:
            self.do_unhandled()
    def do_root(self):
        self.send_response(200)
        self.send_header('Content-type','text/html')
        self.end_headers()

        self.wfile.write('tetrifact server')

    def do_packages(self):
        self.send_response(200)
        self.send_header('Content-type','text/json')
        self.end_headers()

        response = { }
        response['success'] = {}

        with open('./packages.json', 'rb') as file:
            response['success']['packages'] = json.loads(file.read())

        self.wfile.write(json.dumps(response).encode())

    def do_archive(self, archive):
        archivePath = f'./archives/{archive}.zip'

        if not os.path.isfile(archivePath):
            self.send_response(404)
            self.send_header('Content-type','text/html')
            self.end_headers()
            self.wfile.write(f'Archive {archivePath} not found'.encode())
            return

        file_stats = os.stat(archivePath)
        self.send_response(200)
        self.send_header('Content-type','text/json')
        self.send_header('Content-Length', f'{file_stats.st_size}')
        self.end_headers()
        
        with open(archivePath, 'rb') as file: 
            self.wfile.write(file.read()) # Read the file and send the contents 
            
    def do_file(self, id):
        filePath = f'./files/{id}'

        if not os.path.isfile(filePath):
            self.send_response(404)
            self.send_header('Content-type','text/html')
            self.send_header('Content-Length', f'{file_stats.st_size}')
            self.end_headers()
            self.wfile.write(f'file {id} not found'.encode())
            return

        file_stats = os.stat(filePath)
        self.send_response(200)
        self.send_header('Content-type','text/json')
        self.send_header('Content-Length', f'{file_stats.st_size}')
        self.end_headers()

        response = { }
        response['success'] = {}

        with open(filePath, 'rb') as file: 
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

        response = { }
        response['success'] = {}

        with open(packagePath, 'rb') as file:
            response['success']['package'] = json.loads(file.read())

        self.wfile.write(json.dumps(response).encode())

    def do_unhandled(self):
        self.send_response(404)
        self.send_header('Content-type','text/html')
        self.end_headers()

        self.wfile.write('Unhandled route - try something else'.encode())


print(f'Simulating Tetrifact server on port {port}')
server = HTTPServer(('localhost', port), TetrifactServer)
server.serve_forever()