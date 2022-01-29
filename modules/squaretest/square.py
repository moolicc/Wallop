print("Hello world, from a module!")

def update():
    global x
    x += 1

def draw():
    global x
    Image("face.png", x, 0)
    return
    