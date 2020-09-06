from PIL import Image
import sys, math

FRAME_PIXEL_BUFFER = 0

infile = sys.argv[1]

try:
	gif = Image.open(infile)
except IOError:
	print "Unable to open file: {}".format(infile)
	sys.exit(1)

gifWidth = gif.size[0]
gifHeight = gif.size[1]

gifPalette = gif.getpalette()
gifFrames = []
duration = 0

try:
	while(1):
		gif.putpalette(gifPalette)
		
		gifFrame = Image.new("RGB", gif.size)
		gifFrame.paste(gif)
		# gifFrame.save("{}-{}".format(infile, len(gifFrames)), "PNG")

		gifFrames.append(gifFrame)
		duration += gif.info['duration']

		gifPalette = gif.getpalette()
		gif.seek(gif.tell()+1)
except EOFError:
	pass # end of gif	

cols = 4096 / (gifWidth + 10)
rows = float(len(gifFrames)) / float(cols)
rows = int(math.ceil(rows))

print str(cols) + " " + str(rows)
	
gifFramesImage = Image.new(
	"RGB",
	(cols * (gifWidth + FRAME_PIXEL_BUFFER) + FRAME_PIXEL_BUFFER, rows * (gifHeight + FRAME_PIXEL_BUFFER) + FRAME_PIXEL_BUFFER)
)

top = FRAME_PIXEL_BUFFER
left = FRAME_PIXEL_BUFFER
for i in xrange(1, len(gifFrames) + 1):
	gifFramesImage.paste(gifFrames[i-1], (left, top))
	left += gifWidth + FRAME_PIXEL_BUFFER
	if(i%cols == 0):
		left = FRAME_PIXEL_BUFFER
		top += gifHeight + FRAME_PIXEL_BUFFER

gifFramesImage.save("{}-frames.png".format(infile[:len(infile)-4]), "PNG")
print "Frames: " + str(len(gifFrames))
print duration
