ffmpeg -i intro.mp4 -i out.mp4 -filter_complex "[1:v]scale=1280:720,setdar=16/9 [vmain]; [1:a]volume=1.6 [amain]; [0:v]scale=1280:720,setdar=16/9 [vintro]; [vintro][0:a][vmain][amain]concat=n=2:v=1:a=1" -vcodec libx264 -pix_fmt yuv420p -r 30 -g 60 -b:v 1400k -profile:v main -level 3.1 -acodec libmp3lame -b:a 128k -ar 44100 -preset superfast aaaaaaaaaaaa.mp4
