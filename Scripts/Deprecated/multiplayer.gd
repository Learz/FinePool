extends Node

const PORT = 4433

@onready var main_menu: Control = $UI/MainMenu
@onready var player_ui: Control = $UI/PlayerUI

@onready var lobby: Control = $UI/MainMenu/Margin/VBoxContainer/Lobby
@onready var logo: TextureRect = $UI/MainMenu/Margin/VBoxContainer/Options/Logo
@onready var menu_options: Control = $UI/MainMenu/Margin/VBoxContainer/Options/MenuOptions
@onready var player_list: Control = $UI/MainMenu/Margin/VBoxContainer/Lobby/HBoxContainer/PlayerList

# ---- SIGNALS ----

func _on_host_pressed():
	# Start as server
	var peer = ENetMultiplayerPeer.new()
	peer.create_server(PORT)
	if peer.get_connection_status() == MultiplayerPeer.CONNECTION_DISCONNECTED:
		OS.alert("Failed to start multiplayer server")
		return
	multiplayer.multiplayer_peer = peer
	enter_lobby()


func _on_connect_pressed():
	# Start as client
	var txt: String = $UI/MainMenu/Margin/VBoxContainer/Options/MenuOptions/Remote.text
	if txt == "":
		OS.alert("Need a remote to connect to.")
		return
	var peer = ENetMultiplayerPeer.new()
	peer.create_client(txt, PORT)
	if peer.get_connection_status() == MultiplayerPeer.CONNECTION_DISCONNECTED:
		OS.alert("Failed to start multiplayer client")
		return
	multiplayer.multiplayer_peer = peer
	enter_lobby()

func _on_start_game_button_pressed():
	start_game()

func _on_leave_lobby_button_pressed():
	leave_lobby()
	multiplayer.multiplayer_peer = null

func _peer_connected(id: int):
	add_player_to_lobby(id)

func _peer_disconnected(id: int):
	remove_player_from_lobby(id)

func _on_player_name_line_edit_text_submitted(new_text: String):
	print("Renaming player " + str(multiplayer.get_unique_id()) + " to " + new_text)
	if (!player_list.has_node(str(multiplayer.get_unique_id()))):
		print("Player " + str(multiplayer.get_unique_id()) + " not found in " + str(player_list.get_children()))
		return
		
	var lobbyPlayer = player_list.get_node(str(multiplayer.get_unique_id()))

	lobbyPlayer.ChangeName(new_text)
	pass # Replace with function body.

# ---- Overrides ----

func _ready():
	DisplayServer.window_set_min_size(Vector2(800, 600))
	# Start paused
	get_tree().paused = true
	# You can save bandwith by disabling server relay and peer notifications.
	multiplayer.server_relay = false
	multiplayer.peer_connected.connect(_peer_connected)
	multiplayer.peer_disconnected.connect(_peer_disconnected)

	# Automatically start the server in headless mode.
	if DisplayServer.get_name() == "headless":
		print("Automatically starting dedicated server")
		_on_host_pressed.call_deferred()

# The server can restart the level by pressing HOME.
func _input(event):
	if not multiplayer.is_server():
		return

	if event.is_action("ui_home") and Input.is_action_just_pressed("ui_home"):
		change_level.call_deferred(load("res://Scenes/Levels/level.tscn"))

# ---- Functions ----

func enter_lobby():
	menu_options.hide()
	for playerID in multiplayer.get_peers():
		add_player_to_lobby(playerID)
	if multiplayer.is_server() and !OS.has_feature("dedicated_server"):
		add_player_to_lobby(1)
	logo.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT
	lobby.show()

func leave_lobby():
	menu_options.show()
	for lobbyPlayer in player_list.get_children():
		lobbyPlayer.queue_free()
	logo.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	lobby.hide()

func add_player_to_lobby(id: int):
	var lobbyPlayerRes = load("res://Scenes/UI/LobbyPlayer.tscn")
	var lobbyPlayer = lobbyPlayerRes.instantiate()
	lobbyPlayer.name = str(id)
	player_list.add_child(lobbyPlayer)

func remove_player_from_lobby(id: int):
	if (!player_list.has_node(str(id))):
		return
	
	player_list.get_node(str(id)).queue_free()

func start_game():
	# Hide the UI and unpause to start the game.
	main_menu.hide()
	player_ui.show()
	get_tree().paused = false
	# Only change level on the server.
	# Clients will instantiate the level via the spawner.
	if multiplayer.is_server():
		change_level.call_deferred(load("res://Scenes/Levels/level.tscn"))

# Call this function deferred and only on the main authority (server).
func change_level(scene: PackedScene):
	# Remove old level if any.
	var level = $Level
	for c in level.get_children():
		level.remove_child(c)
		c.queue_free()
	# Add new level.
	level.add_child(scene.instantiate())
