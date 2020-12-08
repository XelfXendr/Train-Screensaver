use std::io::prelude::*;
use std::net::{TcpListener, TcpStream};
use std::thread;
use std::sync::mpsc;
use std::collections::VecDeque;
use rand;

fn main() {
    let listener = TcpListener::bind("127.0.0.1:25308").unwrap();
    
    let (sender, receiver) = mpsc::channel::<TcpStream>();

    thread::spawn(move || {communication(receiver)});

    for stream in listener.incoming() {
        let stream = match stream {
            Ok(stream) => stream,
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
        //add incoming streams into
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
                continue;
            },
            Err(_) => continue,
        }

        let mut buffer: [u8; 8] = [0; 8];
        match stream.read(&mut buffer) {
            Ok(n) => if n != 3 {
                continue;
            }
            else {
                match buffer[0] {
                    0x09 => start_pos = ((buffer[1] as u16) << 8) | (buffer[2] as u16),
                    _ => continue,
                }
            },
            Err(_) => continue,
        }
    }
}