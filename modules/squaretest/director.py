print("Hello world, from a director module!")

def update():
    global y
    y += 1

    position = GetComponentByName("Square1", "PositionComponent")
    if not position is None and position.X >= 90:
        position.X = 0
        info("Setting position x to zero.")

def draw():
    global x, y
    Image("face.png", x, y)
    return
