// Firework object
var $Firework1 = $('.firework1');
var $Firework2 = $('.firework2');
var $Firework3 = $('.firework3');
var $Firework4 = $('.firework4');
var $Firework5 = $('.firework5');

const particleCount = 60;

// Color generate
const randomValue = (max, min) => Math.floor(Math.random() * (max - min) + min);
const randomColor = () =>
    `rgba(${randomValue(255, 45)}, ${randomValue(255, 45)}, ${randomValue(255, 45)}, ${randomValue(100, 50)}%)`;

// Genterate particle
const randomPosition = (max, min) => ({
    x: `${randomValue(max, min)}%`,
    y: `${randomValue(max, min)}%`
});
const createParticle = color => `radial-gradient(circle, ${color} 0.2vmin, #0000 0)`;

// Genterate particle style
const particleStyle = (particles, positions) => ({
    backgroundImage: particles.join(','),
    backgroundPositionX: positions.x.join(',').toString(),
    backgroundPositionY: positions.y.join(',').toString()
});

// Generate firework
const createFirework = (firework, n) => {
    var particles = [];
    var positions = {
        x: [],
        y: []
    };

    const color = randomColor();
    for (let i = 0; i < n; i++) {
        const { x, y } = randomPosition(100, 0);
        particles.push(createParticle(color));
        positions.x.push(x);
        positions.y.push(y);
    }

    firework.css({
        ...particleStyle(particles, positions),
        top: `${randomValue(50, 20)}%`,
        left: `${randomValue(80, 30)}%`
    });
};

// Animate settings
const duration = second => second * 1000;
const effect = () => ({
    opacity: 1,
    width: '30vmin'
});
const reset = () => ({
    width: ''
});

const animate = (firework, time) => {
    createFirework(firework, particleCount);
    firework.delay(time).animate(effect(), duration(1.2), 'swing', () => {
        firework.css(reset());
        animate(firework, time);
    })
};

animate($Firework1, duration(0));
animate($Firework2, duration(1));
animate($Firework3, duration(2));
animate($Firework4, duration(3));
animate($Firework5, duration(4));