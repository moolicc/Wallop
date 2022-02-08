print("Hello world, from a director module!")

def update():
    global y
    y += 1

def draw():
    global x, y
    Image("face.png", x, y)
    return
