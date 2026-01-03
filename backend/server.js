const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");

const app = express();
const port = 3000;

app.use(express.json());
app.use(cors());

// --- MONGODB CONNECTION ---
const dbUrl = process.env.DB_URL || "mongodb://mongo:27017/gamedata";
mongoose
	.connect(dbUrl)
	.then(() => console.log("✅ MongoDB Connected"))
	.catch((err) => console.error("❌ MongoDB Error:", err));

// --- DATA SCHEMA ---
const ActionSchema = new mongoose.Schema({
	uid: String,
	questionId: String,
	choiceWeight: Number,
	traitTarget: String,
	responseTimeMs: Number,
	mouseDistance: Number,
	timestamp: { type: Date, default: Date.now },
});

const Action = mongoose.model("Action", ActionSchema);

// --- THE ALGORITHM---
function calculateOceanProfile(uid, actions) {
	// 1. Start with a neutral baseline
	let profile = {
		openness: 50,
		conscientiousness: 50,
		extraversion: 50,
		agreeableness: 50,
		neuroticism: 50,
	};

	if (actions.length > 0) {
		actions.forEach((a) => {
			// IMPACT OF ANSWERS
			if (a.traitTarget === "Agreeableness")
				profile.agreeableness += a.choiceWeight * 12;
			if (a.traitTarget === "Openness") profile.openness += a.choiceWeight * 12;
			if (a.traitTarget === "Conscientiousness")
				profile.conscientiousness += a.choiceWeight * 12;

			// THE UNION TRAP (Question 5)
			if (a.traitTarget === "Agreeableness" && a.choiceWeight < 0) {
				profile.agreeableness -= 20;
			}
		});

		// MOUSE TRACKING (Neuroticism)
		const avgDistance =
			actions.reduce((sum, a) => sum + (a.mouseDistance || 0), 0) /
			actions.length;
		if (avgDistance > 600) profile.neuroticism += 15;
		if (avgDistance > 1200) profile.neuroticism += 30;

		// REACTION TIME
		const avgTime =
			actions.reduce((sum, a) => sum + (a.responseTimeMs || 0), 0) /
			actions.length;

		if (avgTime < 1000) profile.conscientiousness -= 15;
		if (avgTime > 5000) profile.conscientiousness -= 10;
	}

	Object.keys(profile).forEach(
		(k) => (profile[k] = Math.min(100, Math.max(0, profile[k])))
	);

	// --- THE BALANCED VERDICT ---
	let verdict = "HIRE";
	let reason = "Compliance Verified";

	// Rule 1: The Unstable
	if (profile.neuroticism > 70) {
		verdict = "REJECT";
		reason = "High Instability Detected";
	}

	// Rule 2: The Rebels
	if (profile.agreeableness < 40) {
		verdict = "REJECT";
		reason = "Insubordination Risk";
	}

	// Rule 3: The Lazy
	if (profile.conscientiousness < 30) {
		verdict = "REJECT";
		reason = "Inefficiency Detected";
	}

	return { uid, ...profile, verdict, reason };
}

// --- API ROUTES ---

// 1. Receive data from Unity
app.post("/api/track", async (req, res) => {
	try {
		const newAction = new Action(req.body);
		await newAction.save();
		res.json({ status: "success" });
	} catch (error) {
		res.status(500).json({ error: error.message });
	}
});

// 2. Calculate profile for a specific user (End Game)
app.get("/api/profile/:uid", async (req, res) => {
	try {
		const actions = await Action.find({ uid: req.params.uid });
		const profile = calculateOceanProfile(req.params.uid, actions);
		res.json(profile);
	} catch (error) {
		res.status(500).json({ error: error.message });
	}
});

// 3. Admin Dashboard
app.get("/api/dashboard", async (req, res) => {
	try {
		const allActions = await Action.find({});

		const usersData = {};
		allActions.forEach((a) => {
			if (!usersData[a.uid]) usersData[a.uid] = [];
			usersData[a.uid].push(a);
		});

		const summaries = Object.keys(usersData).map((uid) => {
			return calculateOceanProfile(uid, usersData[uid]);
		});

		res.json(summaries);
	} catch (error) {
		res.status(500).json({ error: error.message });
	}
});

app.use("/admin", express.static("public"));

app.listen(port, () => console.log(`🚀 Server running on port ${port}`));
