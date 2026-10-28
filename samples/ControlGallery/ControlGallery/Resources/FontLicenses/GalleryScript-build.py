import sys
from fontTools.ttLib import TTFont

source, target = sys.argv[1], sys.argv[2]
font = TTFont(source)

# Any edit invalidates the digital signature
if 'DSIG' in font:
    del font['DSIG']

name = font['name']
renames = {
    1: ('Pacifico', 'Gallery Script'),
    3: ('Pacifico-Regular', 'GalleryScript-Regular'),
    4: ('Pacifico', 'Gallery Script'),
    6: ('Pacifico-Regular', 'GalleryScript-Regular'),
}
for record in name.names:
    if record.nameID in renames:
        old, new = renames[record.nameID]
        text = record.toUnicode()
        assert old in text, (record.nameID, text)
        record.string = text.replace(old, new)

description = ('Gallery Script is a modified version of Pacifico '
               '(https://github.com/googlefonts/Pacifico): renamed, and with '
               'name records added for the Unicode platform.')
name.setName(description, 10, 1, 0, 0)
name.setName(description, 10, 3, 1, 0x409)

# Unicode platform records, which sort first; the family record holds a shorter name
for name_id, text in {1: 'Script', 2: 'Regular', 4: 'Gallery Script Regular', 6: 'GalleryScript-Regular'}.items():
    name.setName(text, name_id, 0, 0, 0)

font.save(target)
