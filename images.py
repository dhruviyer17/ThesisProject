from google_images_search import GoogleImagesSearch
from io import BytesIO
from PIL import Image
import socket

client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)  # AF_INET = IP, SOCK_STREAM = TCP
client.connect(('localhost', 7000))  # 127.0.0.1

# in this case we're using PIL to keep the BytesIO as an image object
# that way we don't have to wait for disk save / write times
# the image is simply kept in memory
# this example should display 3 pictures of puppies!

gis = GoogleImagesSearch('AIzaSyCXJ9adSwAtUxW5zMHB45Z2q4d0fuMuI2M', '60f513da817364075')

my_bytes_io = BytesIO()

gis.search({'q': '子犬', 'num': 3})
for image in gis.results():
    # here we tell the BytesIO object to go back to address 0
    my_bytes_io.seek(0)

    # take raw image data
    raw_image_data = image.get_raw_data()

    # this function writes the raw image data to the object
    image.copy_to(my_bytes_io, raw_image_data)

    # or without the raw data which will be automatically taken
    # inside the copy_to() method
    image.copy_to(my_bytes_io)

    # we go back to address 0 again so PIL can read it from start to finish
    my_bytes_io.seek(0)

    image_data = my_bytes_io.read(2048)

    
    # create a temporary image object
    temp_img = Image.open(my_bytes_io)
    
    # show it in the default system photo viewer
    temp_img.show()