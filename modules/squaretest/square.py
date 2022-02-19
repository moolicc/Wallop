print("Hello world, from a module!")

def update():
    global x
    x += 1
    panic("Test reason")
    info("test log. shouldn't see this.")

def draw():
    global x, y
    Image("face.png", x, y)
    return
