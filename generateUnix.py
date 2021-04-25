import pathlib

data = []
updatedData = []
path = pathlib.Path(__file__).parent.absolute()

with open(str(path) + "\\classes\\ConvertServer.cs") as myFile:
    data = myFile.readlines()

for line in data:
    line = line.replace("\\\\", "/")
    line = line.replace("ConvertServer", "ConvertServerUnix")
    #line = line.replace("//", "/")
    updatedData.append(line)

with open(str(path) + "\\classes\\ConvertServerUnix.cs", "w") as myWriter:
    for line in updatedData:
        myWriter.write(line)