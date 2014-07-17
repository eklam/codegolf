from PIL import Image
import sys
print sys.argv

def check(palette, copy):
    palette = sorted(Image.open(palette).convert('RGB').getdata())
    copy = sorted(Image.open(copy).convert('RGB').getdata())
    print 'Success' if copy == palette else 'Failed'

check('Goth.png', 'test.png')