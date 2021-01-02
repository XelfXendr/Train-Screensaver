use std::io::prelude::*;
use std::net::{TcpListener, TcpStream};
use std::thread;
use std::sync::mpsc;
use std::collections::{HashMap};
use rand;
use std::env;
use serde::Deserialize;
use serde_json::json;
use std::fs;
use std::fs::File;

fn main() {
    
    let mut dir = env::current_exe().expect("Cannot access executable directory.");
    dir.pop();
    dir.push("config.json");
    
    let config_json = match fs::read_to_string(&dir) {
        Ok(config) => config,
        Err(e) => match e.kind() {
            std::io::ErrorKind::NotFound => 
            {
                let mut file = File::create(dir).expect("Couldn't create configuration file.");
                let json = json!({
                    "address": "0.0.0.0",
                    "port": "25308",
                    "client_order": [
                        "10.0.0.1",
                        "10.0.0.2"
                    ]
                });
                file.write_all(json.to_string().as_bytes()).expect("Couldn't write to configuration file.");

                println!("Please edit configuration file \"config.json\"");

                return;
            }
            _ => panic!("Error reading configuration file: {}", e),
        }
    };

    let c: Config = serde_json::from_str(&config_json).expect("Configuration is invalid. Check for typoes.");
    let server = format!("{}:{}", c.address, c.port);
    let listener = TcpListener::bind(&server).unwrap();
    
    let mut clients: HashMap<String, usize> = HashMap::new();
    for i in 0..c.client_order.len() {
        clients.insert(String::from(&c.client_order[i]), i);
    }

    let (sender, receiver) = mpsc::channel::<TcpStream>();
    thread::spawn(move || {communication(receiver, clients)});

    println!("Server started, listening on {}", server);
    for stream in listener.incoming() {
        let stream = match stream {
            Ok(stream) => 
            {
                println!("New connection: {}", stream.peer_addr().unwrap());
                stream
            },
            Err(err) => {
                println!("Incomming connection failed with error: {}", err);
                continue;
            },
        };

        sender.send(stream).unwrap();
    }
}

fn communication(receiver: mpsc::Receiver<TcpStream>, client_order: HashMap<String, usize>) {
    let mut start_pos: u16 = rand::random::<u16>();
    let mut going_right = true;

    let mut right_stack: Vec<Client> = Vec::new();
    let mut left_stack: Vec<Client> = Vec::new();

    loop {
        //add incoming streams into queue
        loop {
            if right_stack.is_empty() && left_stack.is_empty() {
                match receiver.recv() {
                    Ok(stream) => {
                        let order = match stream.peer_addr() {
                            Ok(addr) => {
                                let ip = addr.ip().to_string();
                                if client_order.contains_key(&ip) {
                                    client_order[&ip]
                                }
                                else {
                                    continue
                                }
                            }
                            Err(_) => continue,
                        };

                        if going_right {
                            left_stack.push(Client {stream, order});
                            left_stack.sort_unstable_by(|a, b| a.order.partial_cmp(&b.order).unwrap());
                        }
                        else {
                            right_stack.push(Client {stream, order});
                            right_stack.sort_unstable_by(|a, b| b.order.partial_cmp(&a.order).unwrap());
                        }
                    },
                    Err(_) => return,
                }
            }
            else {
                match receiver.try_recv() {
                    Ok(stream) => {
                        let order = match stream.peer_addr() {
                            Ok(addr) => {
                                let ip = addr.ip().to_string();
                                if client_order.contains_key(&ip) {
                                    client_order[&ip]
                                }
                                else {
                                    continue
                                }
                            }
                            Err(_) => continue,
                        };

                        if going_right {
                            left_stack.push(Client {stream, order});
                            left_stack.sort_unstable_by(|a, b| a.order.partial_cmp(&b.order).unwrap());
                        }
                        else {
                            right_stack.push(Client {stream, order});
                            right_stack.sort_unstable_by(|a, b| b.order.partial_cmp(&a.order).unwrap());
                        }
                    },
                    Err(e) => match e {
                        mpsc::TryRecvError::Empty => break,
                        mpsc::TryRecvError::Disconnected => return,
                    }, 
                }
            }
        }
        
        let mut client;
        if going_right {
            if right_stack.is_empty() {
                going_right = false;
                continue;
            }
            client = right_stack.pop().unwrap();
        }
        else {
            if left_stack.is_empty() {
                going_right = true;
                continue;
            }
            client = left_stack.pop().unwrap();
        }

        let buffer: [u8; 3] = if going_right { 
            [0x90, (start_pos >> 8) as u8, start_pos as u8] //train right 
        }
        else {
            [0x80, (start_pos >> 8) as u8, start_pos as u8] //train left
        };
        match client.stream.write(&buffer) {
            Ok(n) => if n != 3 {
                println!("Couldn't write 3 bytes to stream.");
                continue;
            },
            Err(e) => {
                println!("Writing failed with error {}.", e);
                continue;
            },
        }

        let mut buffer: [u8; 3] = [0; 3];
        match client.stream.read(&mut buffer) {
            Ok(n) => if n != 3 {
                println!("Received wrong amount of bytes");
                continue;
            }
            else {
                match buffer[0] {
                    0x09 => start_pos = ((buffer[1] as u16) << 8) | (buffer[2] as u16),
                    _ => continue,
                }
            },
            Err(e) => { 
                println!("Receiving failed with error {}", e);
                continue;
            },
        }

        if going_right {
            left_stack.push(client);
        }
        else {
            right_stack.push(client);
        }
    }
}

#[derive(Deserialize)]
struct Config {
    address: String,
    port: u16,
    client_order: Vec<String>,
}

struct Client {
    stream: TcpStream,
    order: usize,
}