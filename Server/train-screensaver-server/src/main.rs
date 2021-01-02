use std::io::prelude::*;
use std::net::{TcpListener, TcpStream};
use std::thread;
use std::sync::mpsc;
use std::collections::VecDeque;
use rand;
use std::env;

fn main() {
    let args: Vec<String> = env::args().collect();

    let listener = TcpListener::bind(&args[1]).unwrap();
    
    let (sender, receiver) = mpsc::channel::<TcpStream>();

    thread::spawn(move || {communication(receiver)});

    println!("Server started, listening on {}", args[1]);
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

fn communication(receiver: mpsc::Receiver<TcpStream>) {
    let mut streams: VecDeque<TcpStream> = VecDeque::new();
    let mut start_pos: u16 = rand::random::<u16>();

    loop {
        //add incoming streams into queue
        loop {
            if streams.is_empty() {
                match receiver.recv() {
                    Ok(stream) => streams.push_back(stream),
                    Err(_) => return,
                }
            }
            else {
                match receiver.try_recv() {
                    Ok(stream) => streams.push_back(stream),
                    Err(e) => match e {
                        mpsc::TryRecvError::Empty => break,
                        mpsc::TryRecvError::Disconnected => return,
                    }, 
                }
            }
        }
        
        let mut stream = streams.pop_front().unwrap();
        let buffer: [u8; 3] = [0x90, (start_pos >> 8) as u8, start_pos as u8]; //trainright

        match stream.write(&buffer) {
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
        match stream.read(&mut buffer) {
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

        streams.push_back(stream);
    }
}