ffmpeg -i test.jpg  -filter:v "drawtext=box=0:fontfile=duy.ttf:text='test chu insert':fontcolor=white:fontsize=95:borderw=7:shadowcolor=blue:shadowx=5:shadowy=10:x=(w-text_w)/2:y=(h-text_h-line_h)/2" -codec:a copy thumbaaaaaaaaaaaaaaaaaaa.jpg