print("Hello world, from a module!")

def update():
    global x
    name = getName()
    if name == "Square1" and x > 100:
        panic("Actor is square1 and x is > 100.")
        return
    x += 1

def draw():
    global x, y
    Image("face.png", x, y)
    return
