print("Hello world, from a module!")

def update():
    global x
    x += 1

def draw():
    global x, y
    Image("face.png", x, y)
    return
